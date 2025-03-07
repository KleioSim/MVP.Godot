using Godot;
using KleioSim.MVP.Godot.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace KleioSim.MVP.Godot;
internal class MVPCore
{
    private static readonly Dictionary<Type, IPresent> viewType2Present = new();
    private static readonly Dictionary<IView, Combine> view2Combine = new();
    private static readonly Dictionary<IView, object> view2Context = new();
    private static readonly Dictionary<Type, object>  present2Mock= new();

    private static object model;

    public static void Initialize()
    {
        var types = typeof(Global).Assembly.DefinedTypes.ToArray();
        foreach (var type in types)
        {
            if (IsPresent(type))
            {
                var viewType = type.BaseType.GetGenericArguments()[0];
                if (viewType2Present.TryGetValue(viewType, out var presentType))
                {
                    throw new Exception();
                }

                viewType2Present.Add(viewType, Activator.CreateInstance(type) as IPresent);
                continue;
            }

            if (IsMock(type))
            {
                var mockAttrib = type.GetCustomAttribute<MockModelAttribute>();

                var modelInterface = mockAttrib.PresentType.BaseType.GetGenericArguments()[1];

                var decoratorMethod = typeof(Decorator).GetMethod("Create").MakeGenericMethod(modelInterface);

                present2Mock.Add(mockAttrib.PresentType, decoratorMethod.Invoke(null, new object[] { Activator.CreateInstance(type) }));
                continue;
            }
        }

        Decorator.OnDataChanged = () =>
        {
            foreach (var combine in view2Combine.Values)
            {
                combine.IsDirty = true;
            }
        };
    }

    private static bool IsMock(TypeInfo type)
    {
        return type.GetCustomAttribute<MockModelAttribute>() != null;
    }

    private static bool IsPresent(TypeInfo type)
    {
        return (type.BaseType != null && type.BaseType.IsGenericType && type.BaseType.GetGenericTypeDefinition() == typeof(Present<,>));
    }

    public static void CreateSession<T>(T model)
    {
        MVPCore.model = Decorator.Create(model);
    }

    internal static void AddViewNode(IView node)
    {
        if (!viewType2Present.TryGetValue(node.GetType(), out var present))
        {
            return;
        }

        var view = (IView)node;
        var combine = new Combine(node as IView, present);
        view2Combine.Add(view, combine);

        foreach (var binding in present.SignalBindings)
        {
            var control = binding.controlGetter.Invoke(view) as Node ?? throw new System.Exception();
            var signal = binding.SignalName as StringName ?? throw new System.Exception();

            control.Connect(signal, Callable.From(() => binding.handlerAction.Invoke(view2Context.GetValueOrDefault(view), model ?? present2Mock.GetValueOrDefault(combine.present.GetType()))));
        }
    }

    internal static void RemoveViewNode(IView view)
    {
        view2Combine.Remove(view);
        view2Context.Remove(view);
    }

    internal static void UpdateViewNodes()
    {
        foreach (var combine in view2Combine.Values)
        {
            if (!combine.IsDirty)
            {
                continue;
            }

            combine.IsDirty = false;

            var currModel = model ?? present2Mock.GetValueOrDefault(combine.present.GetType());
            if (currModel == null)
            {
                continue;
            }

            foreach (var binding in combine.present.UpdateBinding)
            {
                binding.targetSetter.Invoke(combine.view, binding.sourceGetter.Invoke(view2Context.GetValueOrDefault(combine.view), currModel));
            }

            foreach (var binding in combine.present.CollectionBinding)
            {
                var subItemContexts = binding.sourceGetter(view2Context.GetValueOrDefault(combine.view), currModel).ToArray();
                foreach (var itemContext in subItemContexts)
                {
                    var protype = binding.protypeGetter(combine.view);
                    var subViews = protype.GetParent().GetChildren().OfType<IView>().ToArray();
                    var exitSubItems = new List<IView>();
                    foreach (var subView in subViews)
                    {
                        if(view2Context.TryGetValue(subView, out var context))
                        {
                            continue;
                        }

                        if (!subItemContexts.Contains(context))
                        {
                            ((Node)subView).QueueFree();
                        }

                        exitSubItems.Add(subView);
                    }

                    foreach(var subItemConext in subItemContexts)
                    {
                        if (exitSubItems.Contains(subItemConext))
                        {
                            continue;
                        }

                        var subView = protype.CreateInstance() as IView ?? throw new Exception();
                        view2Context.Add(subView, itemContext);
                    }
                }
            }
        }
    }

    internal static void Exit()
    {
        model = null;
        present2Mock.Clear();

        viewType2Present.Clear();
        view2Combine.Clear();

    }
}
