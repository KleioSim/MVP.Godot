using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Godot;
using KleioSim.MVP.Godot.Interfaces;
using MVP.Godot.Bindings;

namespace KleioSim.MVP.Godot;
public abstract class Present<TView, IModel, TContext> : IPresent
    where TView : class, IView
{ 
    private readonly static List<SignalBinding> signalBindings = new();
    private readonly static List<UpdateBinding> updateBinding = new();
    private readonly static List<CollectionBinding> collectionBinding = new();
    private readonly static List<CollectionBinding2> collectionBinding2 = new();
    private readonly static List<TilemapBinding> tilemapBinding = new();
    private readonly static List<PlaceHolderBinding> placeHolderBindings = new();
    private readonly static List<PackedSceneBinding> packedSceneBindings = new();
    public IEnumerable<SignalBinding> SignalBindings => signalBindings;
    public IEnumerable<UpdateBinding> UpdateBinding => updateBinding;

    public IEnumerable<CollectionBinding> CollectionBinding => collectionBinding;

    public IEnumerable<TilemapBinding> TilemapBindings => tilemapBinding;

    public IEnumerable<CollectionBinding2> CollectionBinding2 => collectionBinding2;

    public IEnumerable<PlaceHolderBinding> PlaceHolderBindings => placeHolderBindings;

    public IEnumerable<PackedSceneBinding> PackedSceneBindings => packedSceneBindings;

    public static void BindProperty<TData>(
        Expression<Func<TView, TData>> targetExpr,
        Func<TContext, IModel, TData> sourceGetter)
    {
        var instanceParameter = targetExpr.Parameters.Single();
        var valueParameter = System.Linq.Expressions.Expression.Parameter(typeof(TData), "value");

        var targetSetter = System.Linq.Expressions.Expression.Lambda<Action<TView, TData>>(
                System.Linq.Expressions.Expression.Assign(targetExpr.Body, valueParameter),
                instanceParameter,
                valueParameter)
            .Compile() ?? throw new System.InvalidOperationException();

        updateBinding.Add(new UpdateBinding((view, data) => targetSetter((TView)view, (TData)data), (object context, object mdoel) => sourceGetter((TContext)context, (IModel)mdoel)));
    }

    public static void BindSignal<TControl>(Expression<Func<TView, TControl>> controlExpr, object SignalName, Action<TContext, IModel> action)
    {
        var contrlGetter = controlExpr.Compile() ?? throw new System.InvalidOperationException();

        signalBindings.Add(new SignalBinding((obj)=>contrlGetter((TView)obj), SignalName,(object obj1, object obj2)=> action((TContext)obj1, (IModel)obj2)));
    }

    public static void BindCollection<TData>(Expression<Func<TView, InstancePlaceholder>> protypeExpr, Expression<Func<TContext, IModel, IEnumerable<TData>>> sourceExpr)
    {
        var protypeGetter = protypeExpr.Compile() ?? throw new System.InvalidOperationException();
        var sourceGetter = sourceExpr.Compile() ?? throw new System.InvalidOperationException();

        collectionBinding.Add(new CollectionBinding((view) => protypeGetter((TView)view), (object context, object mdoel) => sourceGetter((TContext)context, (IModel)mdoel).Select(x=>(object)x)));
    }

    public static void BindCollection<TData>(Expression<Func<TView, ICollectionView>> protypeExpr, Expression<Func<TContext, IModel, IEnumerable<TData>>> sourceExpr)
    {
        var protypeGetter = protypeExpr.Compile() ?? throw new System.InvalidOperationException();
        var sourceGetter = sourceExpr.Compile() ?? throw new System.InvalidOperationException();

        collectionBinding2.Add(new CollectionBinding2((view) => protypeGetter((TView)view), (object context, object mdoel) => sourceGetter((TContext)context, (IModel)mdoel).Select(x => (object)x)));
    }
}

public abstract class Present<TView, IModel> : IPresent
    where TView : class, IView
{
    private readonly static List<SignalBinding> signalBindings = new();
    private readonly static List<UpdateBinding> updateBinding = new();
    private readonly static List<CollectionBinding> collectionBinding = new();
    private readonly static List<TilemapBinding> tilemapBinding = new();
    private readonly static List<CollectionBinding2> collectionBinding2 = new();
    private readonly static List<PlaceHolderBinding> placeHolderBindings = new();
    private readonly static List<PackedSceneBinding> packedSceneBindings = new();

    public IEnumerable<SignalBinding> SignalBindings => signalBindings;
    public IEnumerable<UpdateBinding> UpdateBinding => updateBinding;
    public IEnumerable<CollectionBinding> CollectionBinding => collectionBinding;
    public IEnumerable<TilemapBinding> TilemapBindings => tilemapBinding;
    public IEnumerable<CollectionBinding2> CollectionBinding2 => collectionBinding2;
    public IEnumerable<PlaceHolderBinding> PlaceHolderBindings => placeHolderBindings;
    public IEnumerable<PackedSceneBinding> PackedSceneBindings => packedSceneBindings;

    public static void BindProperty<TData>(
        Expression<Func<TView, TData>> targetExpr,
        Func<IModel, TData> sourceGetter)
    {
        var instanceParameter = targetExpr.Parameters.Single();
        var valueParameter = System.Linq.Expressions.Expression.Parameter(typeof(TData), "value");

        var targetSetter = System.Linq.Expressions.Expression.Lambda<Action<TView, TData>>(
                System.Linq.Expressions.Expression.Assign(targetExpr.Body, valueParameter),
                instanceParameter,
                valueParameter)
            .Compile() ?? throw new System.InvalidOperationException();

        updateBinding.Add(new UpdateBinding((view, data) => targetSetter((TView)view, (TData)data), (object context, object mdoel) => sourceGetter((IModel)mdoel)));
    }

    public static void BindSignal<TControl>(Expression<Func<TView, TControl>> controlExpr, object SignalName, Expression<Action<IModel>> actionExpr)
    {
        var contrlGetter = controlExpr.Compile() ?? throw new System.InvalidOperationException();
        var action = actionExpr.Compile() ?? throw new System.InvalidOperationException();

        signalBindings.Add(new SignalBinding((obj) => contrlGetter((TView)obj), SignalName, (object obj1, object obj2) => action((IModel)obj2)));
    }

    public static void BindCollection<TData>(Expression<Func<TView, InstancePlaceholder>> protypeExpr, Expression<Func<IModel, IEnumerable<TData>>> sourceExpr)
    {
        var protypeGetter = protypeExpr.Compile() ?? throw new System.InvalidOperationException();
        var sourceGetter = sourceExpr.Compile() ?? throw new System.InvalidOperationException();

        collectionBinding.Add(new CollectionBinding((view) => protypeGetter((TView)view), (object context, object mdoel) => sourceGetter((IModel)mdoel).Select(x => (object)x)));
    }

    public static void BindTileMap<TData>(Expression<Func<TView, TileMapLayer>> controlExpr, Expression<Func<IModel, IReadOnlyDictionary<Vector2I, TData>>> sourceExpr)
    {
        var tilemapGetter = controlExpr.Compile() ?? throw new System.InvalidOperationException();
        var sourceGetter = sourceExpr.Compile() ?? throw new System.InvalidOperationException();

        tilemapBinding.Add(new TilemapBinding((view) => tilemapGetter((TView)view), (object context, object mdoel) => sourceGetter((IModel)mdoel).ToDictionary(p=>p.Key, p=> (object)p.Value)));
    }

    public static void BindCollection<TData>(Expression<Func<TView, ICollectionView>> protypeExpr, Expression<Func<IModel, IEnumerable<TData>>> sourceExpr)
    {
        var protypeGetter = protypeExpr.Compile() ?? throw new System.InvalidOperationException();
        var sourceGetter = sourceExpr.Compile() ?? throw new System.InvalidOperationException();

        collectionBinding2.Add(new CollectionBinding2((view) => protypeGetter((TView)view), (object context, object mdoel) => sourceGetter((IModel)mdoel).Select(x => (object)x)));
    }

    public static void BindPlaceHolder<TData>(Expression<Func<TView, InstancePlaceholder>> protypeExpr, Expression<Func<IModel, TData>> sourceExpr)
    {
        var protypeGetter = protypeExpr.Compile() ?? throw new System.InvalidOperationException();
        var sourceGetter = sourceExpr.Compile() ?? throw new System.InvalidOperationException();

        placeHolderBindings.Add(new PlaceHolderBinding((view) => protypeGetter((TView)view), (object context, object mdoel) => sourceGetter((IModel)mdoel)));
    }

    public static void BindPackedScene<TData>(Expression<Func<TView, PackedScene>> protypeExpr, Func<IModel, TData> sourceGetter)
    {
        var protypeGetter = protypeExpr.Compile() ?? throw new System.InvalidOperationException();
        packedSceneBindings.Add(new PackedSceneBinding((view) => protypeGetter((TView)view), (object context, object mdoel) => sourceGetter((IModel)mdoel)));
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