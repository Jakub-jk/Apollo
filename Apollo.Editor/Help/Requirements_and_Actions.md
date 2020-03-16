# *Requirements* and *Actions*

_**WARNING: Advanced stuff over here. See [Variables](Variables) and [Expressions](Expressions) first.**_

## Requirements

Requirements allow you to decide whether certain dialog option is available to choose or not.

*Requirement* allows you to enter an expression to evaluate. If it evaluates to `True`, dialog option would be available for user to select, eg.:
```
[1] Option 1
[2] Option 2
[3] Option 3
```
If it evaluates to `False` however, user won't even be able to see it, for example if `Option 2` evaluates to `False`:
```
[1] Option 1
[2] Option 3
```

### Example
If you would like to set an option to be available only when variable `Money` is above 100, you would set an expression like this:
```
@{Money} > 100
```

## Actions

*Actions* allow you to dynamically execute certain expressions **before** content is printed onto the screen, which means that if you change variable here, new value will be displayed in text.

*Actions*, as opposed to *Requirements*, allow you to have multiple expressions - you just need to put them in separate lines.

### Example
Imagine that hero of your story buys a sword, but it costs him 100 coins. To change variables `Has Sword`, which indicates that he bought it, and `Money`, you would write *Actions* like this:
```
$set("Has Sword", 1)
$set("Money", @{Money} - 100)
```