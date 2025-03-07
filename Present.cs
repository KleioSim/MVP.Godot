using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Godot;
using KleioSim.MVP.Godot.Interfaces;
using MVP.Godot.Bindings;

namespace KleioSim.MVP.Godot;
public abstract class Present<TView, IModel> : IPresent
    where TView : class, IView
{ 
    private readonly static List<SignalBinding> signalBindings = new();
    private readonly static List<UpdateBinding> updateBinding = new();
    private readonly static List<CollectionBinding> collectionBinding = new();

    public IEnumerable<SignalBinding> SignalBindings => signalBindings;
    public IEnumerable<UpdateBinding> UpdateBinding => updateBinding;

    public IEnumerable<CollectionBinding> CollectionBinding => collectionBinding;

    public static void BindProperty<TData>(
        Expression<Func<TView, TData>> targetExpr,
        Expression<Func<object, IModel, TData>> sourceExpr)
    {
        var sourceGetter = sourceExpr.Compile() ?? throw new System.InvalidOperationException();

        var instanceParameter = targetExpr.Parameters.Single();
        var valueParameter = System.Linq.Expressions.Expression.Parameter(typeof(TData), "value");

        var targetSetter = System.Linq.Expressions.Expression.Lambda<Action<TView, TData>>(
                System.Linq.Expressions.Expression.Assign(targetExpr.Body, valueParameter),
                instanceParameter,
                valueParameter)
            .Compile() ?? throw new System.InvalidOperationException();

        updateBinding.Add(new UpdateBinding((view, data) => targetSetter((TView)view, (TData)data), (object context, object mdoel) => sourceGetter(context, (IModel)mdoel)));
    }

    public static void BindSignal<TControl>(Expression<Func<TView, TControl>> controlExpr, object SignalName, Expression<Action<object, IModel>> actionExpr)
    {
        var contrlGetter = controlExpr.Compile() ?? throw new System.InvalidOperationException();
        var action = actionExpr.Compile() ?? throw new System.InvalidOperationException();

        signalBindings.Add(new SignalBinding((obj)=>contrlGetter((TView)obj), SignalName,(object obj1, object obj2)=> action(obj1, (IModel)obj2)));
    }

    public static void BindCollection<TData>(Expression<Func<TView, InstancePlaceholder>> protypeExpr, Expression<Func<object, IModel, IEnumerable<TData>>> sourceExpr)
    {
        var protypeGetter = protypeExpr.Compile() ?? throw new System.InvalidOperationException();
        var sourceGetter = sourceExpr.Compile() ?? throw new System.InvalidOperationException();

        collectionBinding.Add(new CollectionBinding((view) => protypeGetter((TView)view), (object context, object mdoel) => sourceGetter(context, (IModel)mdoel).Select(x=>(object)x)));
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