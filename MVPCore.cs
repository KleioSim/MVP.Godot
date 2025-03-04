using Godot;
using KleioSim.MVP.Godot.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;


namespace KleioSim.MVP.Godot;
internal class MVPCore
{
    private static readonly Dictionary<Type, IPresent> view2Present = new();
    private static readonly Dictionary<IView, Combine> view2combines = new();

    private static object model;

    public static void Initialize()
    {
        var types = typeof(Global).Assembly.DefinedTypes.ToArray();
        foreach (var type in types)
        {
            if (type.BaseType == null || !type.BaseType.IsGenericType || type.BaseType.GetGenericTypeDefinition() != typeof(Present<,>))
            {
                continue;
            }

            var viewType = type.BaseType.GetGenericArguments()[0];
            if (view2Present.TryGetValue(viewType, out var presentType))
            {
                throw new Exception();
            }

            view2Present.Add(viewType, Activator.CreateInstance(type) as IPresent);
        }

        Decorator.OnDataChanged = () =>
        {
            foreach (var combine in view2combines.Values)
            {
                combine.IsDirty = true;
            }
        };
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
        if (model == null)
        {
            return;
        }

        foreach (var combine in view2combines.Values)
        {
            if (combine.IsDirty)
            {
                combine.IsDirty = false;
                foreach (var binding in combine.present.UpdateBinding)
                {
                    binding.targetSetter.Invoke(combine.view, binding.sourceGetter.Invoke(combine.context, model));
                }
            }
        }
    }

    internal static void Exit()
    {
        model = null;

        view2Present.Clear();
        view2combines.Clear();
    }
}
