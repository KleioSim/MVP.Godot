using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Godot;
using KleioSim.MVP.Godot.Interfaces;
using MVP.Godot.Bindings;

using Expression = System.Linq.Expressions.Expression;

namespace KleioSim.MVP.Godot;
public abstract class Present<TView, IModel, TContext> :  Present<TView, IModel>
    where TView : class, IView
{ 
    public static void BindProperty<TData>(
        Expression<Func<TView, TData>> targetExpr,
        Func<TContext, IModel, TData> sourceGetter,
        bool freshOnlyVisable = true)
    {
        var instanceParameter = targetExpr.Parameters.Single();
        var valueParameter = Expression.Parameter(typeof(TData), "value");

        var targetSetter = Expression.Lambda<Action<TView, TData>>(
                Expression.Assign(targetExpr.Body, valueParameter),
                instanceParameter,
                valueParameter)
            .Compile();

        updateBinding.Add(new UpdateBinding((view, data) => targetSetter((TView)view, (TData)data), (object context, object mdoel) => sourceGetter((TContext)context, (IModel)mdoel), freshOnlyVisable));
    }

    public static void BindSignal<TControl>(Expression<Func<TView, TControl>> controlExpr, object SignalName, Action<TContext, IModel> action)
    {
        var contrlGetter = controlExpr.Compile();

        signalBindings.Add(new SignalBinding((obj)=>contrlGetter((TView)obj), SignalName,(object obj1, object obj2)=> action((TContext)obj1, (IModel)obj2)));
    }

    public static void BindCollection<TData>(Expression<Func<TView, InstancePlaceholder>> protypeExpr, Func<TContext, IModel, IEnumerable<TData>> sourceExpr)
    {
        var protypeGetter = protypeExpr.Compile() ;
        var sourceGetter = sourceExpr;

        collectionBinding.Add(new CollectionBinding((view) => protypeGetter((TView)view), (object context, object mdoel) => sourceGetter((TContext)context, (IModel)mdoel).Select(x=>(object)x)));
    }

    public static void BindPlaceHolder<TData>(Expression<Func<TView, InstancePlaceholder>> protypeExpr, Expression<Func<TContext, IModel, TData>> sourceExpr)
    {
        var protypeGetter = protypeExpr.Compile() ;
        var sourceGetter = sourceExpr.Compile() ;

        placeHolderBindings.Add(new PlaceHolderBinding((view) => protypeGetter((TView)view), (object context, object mdoel) => sourceGetter((TContext)context, (IModel)mdoel)));
    }
}

public abstract class Present<TView, IModel> : IPresent
    where TView : class, IView
{
    protected readonly static List<SignalBinding> signalBindings = new();
    protected readonly static List<UpdateBinding> updateBinding = new();
    protected readonly static List<CollectionBinding> collectionBinding = new();
    protected readonly static List<TilemapBinding> tilemapBinding = new();
    protected readonly static List<PlaceHolderBinding> placeHolderBindings = new();
    protected readonly static List<PackedSceneBinding> packedSceneBindings = new();

    public IEnumerable<SignalBinding> SignalBindings => signalBindings;
    public IEnumerable<UpdateBinding> UpdateBinding => updateBinding;
    public IEnumerable<CollectionBinding> CollectionBinding => collectionBinding;
    public IEnumerable<TilemapBinding> TilemapBindings => tilemapBinding;
    public IEnumerable<PlaceHolderBinding> PlaceHolderBindings => placeHolderBindings;
    public IEnumerable<PackedSceneBinding> PackedSceneBindings => packedSceneBindings;

    public static void BindProperty<TData>(
        Expression<Func<TView, TData>> targetExpr,
        Func<IModel, TData> sourceGetter,
        bool freshOnlyVisiable = true)
    {
        var instanceParameter = targetExpr.Parameters.Single();
        var valueParameter = Expression.Parameter(typeof(TData), "value");

        var targetSetter = Expression.Lambda<Action<TView, TData>>(
                Expression.Assign(targetExpr.Body, valueParameter),
                instanceParameter,
                valueParameter)
            .Compile() ;

        updateBinding.Add(new UpdateBinding(
            (view, data) => targetSetter((TView)view, (TData)data), 
            (object context, object mdoel) => sourceGetter((IModel)mdoel),
            freshOnlyVisiable));
    }

    public static void BindSignal<TControl>(Expression<Func<TView, TControl>> controlExpr, object SignalName, Action<IModel> action)
    {
        var contrlGetter = controlExpr.Compile() ;

        signalBindings.Add(new SignalBinding(
            (obj) => contrlGetter((TView)obj), 
            SignalName, 
            (object obj1, object obj2) => action((IModel)obj2)));
    }

    public static void BindCollection<TData>(Expression<Func<TView, InstancePlaceholder>> protypeExpr, Expression<Func<IModel, IEnumerable<TData>>> sourceExpr)
    {
        var protypeGetter = protypeExpr.Compile() ;
        var sourceGetter = sourceExpr.Compile() ;

        collectionBinding.Add(new CollectionBinding(
            (view) => protypeGetter((TView)view), 
            (object context, object mdoel) => sourceGetter((IModel)mdoel).Select(x => (object)x)));
    }

    public static void BindTileMap<TData>(Expression<Func<TView, TileMapLayer>> controlExpr, Expression<Func<IModel, IReadOnlyDictionary<Vector2I, TData>>> sourceExpr)
    {
        var tilemapGetter = controlExpr.Compile() ;
        var sourceGetter = sourceExpr.Compile() ;

        tilemapBinding.Add(new TilemapBinding(
            (view) => tilemapGetter((TView)view), 
            (object context, object mdoel) => sourceGetter((IModel)mdoel).ToDictionary(p=>p.Key, p=> (object)p.Value)));
    }

    public static void BindPlaceHolder<TData>(Expression<Func<TView, InstancePlaceholder>> protypeExpr, Expression<Func<IModel, TData>> sourceExpr)
    {
        var protypeGetter = protypeExpr.Compile() ;
        var sourceGetter = sourceExpr.Compile() ;

        placeHolderBindings.Add(new PlaceHolderBinding(
            (view) => protypeGetter((TView)view), 
            (object context, object mdoel) => sourceGetter((IModel)mdoel)));
    }

    public static void BindPackedScene<TData>(Expression<Func<TView, PackedScene>> protypeExpr, Func<IModel, TData> sourceGetter)
    {
        var protypeGetter = protypeExpr.Compile() ;
        packedSceneBindings.Add(new PackedSceneBinding(
            (view) => protypeGetter((TView)view), 
            (object context, object mdoel) => sourceGetter((IModel)mdoel)));
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

[AttributeUsage(AttributeTargets.Class)]
public class MockContextAttribute : Attribute
{
    public string MockPropertyName { get; }

    public MockContextAttribute(string mockPropertyName)
    {
        MockPropertyName = mockPropertyName;
    }
}