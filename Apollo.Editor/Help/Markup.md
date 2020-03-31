# Markup
## Syntax
##### Without parameters
<code>\`tag\`</code>
##### With parameters
<code>\`tag:parameter\`</code>
## Available tags
|  Tag | Parameters | Meaning |
| - | - | - |
| `b` | See **[Colors](Colors "Colors")** | Set background color |
| `f` | See **[Colors](Colors "Colors")** | Set text color |
| `c` | `b`, `f` or none | Set colors to default, respective to given parameters (corresponding to tags above) or both, if none parameter is given.|
| `e` | See **[Expressions](Expressions "Expressions")** | Evaluate expression and place its output in text <br> **You can use only one line** |
| `r` | None | Swap background and text colors |
| `v` | Variable name | Insert variable's current value into text |
