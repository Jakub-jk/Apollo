# Expressions
You should read [Variables](Variables) first.

## Available data types
- Text - **must** be placed in quotes
- Numbers - integers or decimals
- Booleans
	- True - Any number above 0 or "True", case doesn't matter
	- False - 0 or "False", case doesn't matter

## Functions
To use a function, you must prepend it with `$`.

|Function|Parameters|Meaning|Example|
|-|-|-|-|
|`set`|Variable name **in quotes**, value|Sets value of variable|`$set("Money", 100)`|
|`text`|[Color](Colors)|Sets text color|`$text(12)`|
|`back`|[Color](Colors)|Sets background color|`$back(12)`|
|`inv`|None|Inverts colors|`$inv()`|
|`clcr`|"f" *or* "b" *or* none|Sets **f**oreground or **b**ackground color, or both if no parameter is given, to default|`$clcr("f")`|
|`mul`|Text, number|Multiplies `text` `number` times|`$mul("Hey! ", 3)`|

## Math
`+` - addition (and string concatenation)
`-` - subtraction
`/` - division
`*` - multiplication
`$pow(num, power)` - power
`$abs(num)` - absolute value

## Logic
`=` or `==` - equals
`>` - greater than
`>=` - greater than or equal
`<` - less than
`<=` - less than or equal
`!=` - not equal
`&&` - logical AND
`||` - logical OR
`$if(condition, ifTrue, ifFalse)` - evaluates expression and performs action depending on result
`$case(condition1, action1, condition2, action2,...,"else", defAction)` - performs action of first matching expression, or "else" (must be in quotes) if is defined

## Unary operators
To use unary operator with a variable or function, like `-` (number negation) or `!` (bool negation), you must enclose variable/function in parenthesis.
### Example
To negate given value, you simply type:
```
-100
!0
```
But if you need to negate function result or variable, you type:
```
-($pow(2, 3))
!(@{someBool})
```
Typing it as:
```
-$pow(2, 3)
!@{someBool}
```
will result in error.