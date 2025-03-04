using Godot;
using KleioSim.MVP.Godot.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace KleioSim.MVP.Godot;
internal class MVPCore
{
    private static readonly Dictionary<Type, IPresent> view2Present = new();
    private static readonly Dictionary<IView, Combine> view2combines = new();
    private static readonly Dictionary<Type, object>  viewType2Mock= new();

    private static object model;

    public static void Initialize()
    {
        var types = typeof(Global).Assembly.DefinedTypes.ToArray();
        foreach (var type in types)
        {
            if (IsPresent(type))
            {
                var viewType = type.BaseType.GetGenericArguments()[0];
                if (view2Present.TryGetValue(viewType, out var presentType))
                {
                    throw new Exception();
                }

                view2Present.Add(viewType, Activator.CreateInstance(type) as IPresent);
                continue;
            }

            if (IsMock(type))
            {
                var mockAttrib = type.GetCustomAttribute<MockModelAttribute>();
                viewType2Mock.Add(mockAttrib.ViewType, Activator.CreateInstance(type));
                continue;
            }
        }

        Decorator.OnDataChanged = () =>
        {
            foreach (var combine in view2combines.Values)
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
        if (!view2Present.TryGetValue(node.GetType(), out var present))
        {
            return;
        }

        var view = (IView)node;
        var combine = new Combine(node as IView, present);
        view2combines.Add(view, combine);

        foreach (var binding in present.SignalBindings)
        {
            var control = binding.controlGetter.Invoke(view) as Control ?? throw new System.Exception();
            var signal = binding.SignalName as StringName ?? throw new System.Exception();

            control.Connect(signal, Callable.From(() => binding.handlerAction.Invoke(combine.context, model)));
        }
    }

    internal static void RemoveViewNode(IView view)
    {
        view2combines.Remove(view);
    }

    internal static void UpdateViewNodes()
    {
        foreach (var combine in view2combines.Values)
        {
            var currModel = model ?? viewType2Mock.GetValueOrDefault(combine.view.GetType());

            if (combine.IsDirty)
            {
                combine.IsDirty = false;
                foreach (var binding in combine.present.UpdateBinding)
                {
                    binding.targetSetter.Invoke(combine.view, binding.sourceGetter.Invoke(combine.context, currModel));
                }
            }
        }
    }

    internal static void Exit()
    {
        model = null;
        viewType2Mock.Clear();

        view2Present.Clear();
        view2combines.Clear();

    }
}
