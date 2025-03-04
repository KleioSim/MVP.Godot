using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using KleioSim.MVP.Godot.Interfaces;

namespace KleioSim.MVP.Godot;
public abstract class Present<TView, IModel> : IPresent
    where TView : class, IView
{ 
    private readonly static List<SignalBinding> signalBindings = new();
    private readonly static List<UpdateBinding> updateBinding = new();

    public IEnumerable<SignalBinding> SignalBindings => signalBindings;
    public IEnumerable<UpdateBinding> UpdateBinding => updateBinding;

    public static void BindProperty<TData>(
        Expression<Func<TView, TData>> targetExpr,
        Expression<Func<Context, IModel, TData>> sourceExpr)
    {
        var sourceGetter = sourceExpr.Compile() ?? throw new System.InvalidOperationException();

        var instanceParameter = targetExpr.Parameters.Single();
        var valueParameter = System.Linq.Expressions.Expression.Parameter(typeof(TData), "value");

        var targetSetter = System.Linq.Expressions.Expression.Lambda<Action<TView, TData>>(
                System.Linq.Expressions.Expression.Assign(targetExpr.Body, valueParameter),
                instanceParameter,
                valueParameter)
            .Compile() ?? throw new System.InvalidOperationException();

        updateBinding.Add(new UpdateBinding((view, data) => targetSetter((TView)view, (TData)data), (Context context, object mdoel) => sourceGetter(context, (IModel)mdoel)));
    }

    public static void BindSignal<TControl>(Expression<Func<TView, TControl>> controlExpr, object SignalName, Expression<Action<Context, IModel>> actionExpr)
    {
        var contrlGetter = controlExpr.Compile() ?? throw new System.InvalidOperationException();
        var action = actionExpr.Compile() ?? throw new System.InvalidOperationException();

        signalBindings.Add(new SignalBinding((obj)=>contrlGetter((TView)obj), SignalName,(Context obj1, object obj2)=> action(obj1, (IModel)obj2)));
    }
}

[AttributeUsage(AttributeTargets.Class)]
public class MockModelAttribute : Attribute
{
    public Type PresentType { get; }

    public MockModelAttribute(Type type)
    {
        PresentType = type;
    }
}