﻿namespace Money.Api.Tests.TestTools;

public abstract class TestObject
{
    private readonly List<TestObject> _objects = [];

    public DatabaseClient Environment { get; private set; } = null!;
    protected bool IsNew { get; set; }

    public virtual void Attach(DatabaseClient env)
    {
        Environment = env;
        AfterAttach();
        env.AddObject(this);
    }

    public virtual void AfterAttach()
    {
    }

    public abstract void LocalSave();

    public TestObject SaveObject()
    {
        LocalSave();

        foreach (TestObject testObject in _objects)
        {
            testObject.SaveObject();
        }

        return this;
    }
}

public abstract class ReadonlyTestObject : TestObject
{
    public override void LocalSave()
    {
    }
}
