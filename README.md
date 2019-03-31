Merror
======

A simple C# Reflection wrapper to access to private class members.

## How to use

You can make a `Reflector` instance with the target type

```cs
var reflector = new Reflector(typeof(TargetClass));
```

or the target type name and another type in the target assembly when the target class isn't public.

```cs
var reflector = new Reflector("FooNameSpace.TargetClass", typeof(AnotherClass));
```

Get a static field value.

```cs
var val = reflector.GetStaticField("fieldName");
```

Create an instance, and invoke a method.

```cs
var paramOfConstructor = "abc";
var obj = reflector.NewInstance(paramOfConstructor);
var result = (int)reflector.Invoke(obj, "Sum", 123, 456);
```

In some cases, members not found with passed parameters. In that case, try to use `ReflectorParam` and Exact methods.

```cs
var paramOfConstructor = ReflectorParam.New<string>(null);
var obj = reflector.NewInstanceExact(paramOfConstructor);

var inParam1 = ReflectorParam.New(123);
var inParam2 = ReflectorParam.New(456);
var outParam = ReflectorParam.New(0, true);
reflector.InvokeExact(obj, "Sum", inParam1, inParam2, outParam);
var result = (int)outParam.Value;
```
