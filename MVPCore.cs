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
    private static readonly Dictionary<Type, object>  present2ModelMock= new();
    private static readonly Dictionary<Type, object> present2ContextMock = new();

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

            if (IsModelMock(type))
            {
                var mockAttrib = type.GetCustomAttribute<MockModelAttribute>();

                var modelMock = Activator.CreateInstance(type);
                var modelInterface = mockAttrib.PresentType.BaseType.GetGenericArguments()[1];

                var decoratorMethod = typeof(Decorator).GetMethod("Create").MakeGenericMethod(modelInterface);

                present2ModelMock.Add(mockAttrib.PresentType, decoratorMethod.Invoke(null, new object[] { modelMock }));

                var contextAttrib = type.GetCustomAttribute<MockContextAttribute>();
                if (contextAttrib != null)
                {
                    present2ContextMock.Add(mockAttrib.PresentType, modelMock.GetType().GetProperty(contextAttrib.MockPropertyName).GetValue(modelMock));
                }
                
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

    private static bool IsModelMock(TypeInfo type)
    {
        return type.GetCustomAttribute<MockModelAttribute>() != null;
    }

    private static bool IsPresent(TypeInfo type)
    {
        return (type.BaseType != null && type.BaseType.IsGenericType 
            && (type.BaseType.GetGenericTypeDefinition() == typeof(Present<,>) || type.BaseType.GetGenericTypeDefinition() == typeof(Present<,,>)));
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

            control.Connect(signal, Callable.From(() => binding.handlerAction.Invoke(view2Context.GetValueOrDefault(view), model ?? present2ModelMock.GetValueOrDefault(combine.present.GetType()))));
        }
    }

    internal static void RemoveViewNode(IView view)
    {
        view2Combine.Remove(view);
        view2Context.Remove(view);
    }

    internal static void SyncContext(IView source, IView target)
    {
        var context = view2Context.GetValueOrDefault(source);
        if(context != null)
        {
            view2Context.Add(target, context);
        }
    }

    internal static void UpdateViewNodes()
    {
        foreach (var combine in view2Combine.Values.Where(x=>x.IsDirty).ToArray())
        {
            combine.IsDirty = false;

            object currModel = model;
            object context = null;

            if (currModel == null)
            {
                currModel = present2ModelMock.GetValueOrDefault(combine.present.GetType());

                context = present2ContextMock.GetValueOrDefault(combine.present.GetType());
            }
            else
            {
                context = view2Context.GetValueOrDefault(combine.view);
            }


            foreach (var binding in combine.present.UpdateBinding)
            {
                binding.targetSetter.Invoke(combine.view, binding.sourceGetter.Invoke(context, currModel));
            }

            foreach (var binding in combine.present.CollectionBinding)
            {
                var subItemContexts = binding.sourceGetter(context, currModel).ToArray();

                var protype = binding.protypeGetter(combine.view);
                var currItemViews = protype.GetParent().GetChildren().OfType<IView>().ToArray();
                var exitSubItems = new Dictionary<IView, object>();

                foreach (var ItemView in currItemViews)
                {
                    if (!view2Context.TryGetValue(ItemView, out var ItemContext))
                    {
                        continue;
                    }

                    if (!subItemContexts.Contains(ItemContext))
                    {
                        ((Node)ItemView).QueueFree();
                        continue;
                    }

                    exitSubItems.Add(ItemView, ItemContext);
                }

                foreach (var subItemConext in subItemContexts)
                {
                    if (exitSubItems.Values.Contains(subItemConext))
                    {
                        continue;
                    }
                    
                    var subView = protype.CreateInstance() as IView ?? throw new Exception();
                    view2Context.Add(subView, subItemConext);
                }
            }
        }
    }

    internal static void Exit()
    {
        model = null;
        present2ModelMock.Clear();

        viewType2Present.Clear();
        view2Combine.Clear();

    }
}
