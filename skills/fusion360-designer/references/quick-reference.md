# Fusion 360 CLI Quick Reference

## All Commands

### Setup
| Command | Description |
|---------|-------------|
| `status` | Check connection |
| `install [path]` | Install FusionMCP plugin |
| `info` | Full design info |
| `bodies` | List bodies |
| `faces [body]` | List faces |
| `edges [body]` | List edges |
| `timeline` | Feature timeline |
| `help` | Show help |

### Sketch
| Command | Args |
|---------|------|
| `sketch` | `[plane] [name]` |
| `sketch-face` | `<body> <face>` |
| `finish-sketch` | `[sketch]` |
| `delete-sketch` | `[sketch]` |

### Drawing
| Command | Args |
|---------|------|
| `rect` | `<x1> <y1> <x2> <y2>` |
| `center-rect` | `<cx> <cy> <w> <h>` |
| `circle` | `<cx> <cy> <r>` |
| `line` | `<x1> <y1> <x2> <y2>` |
| `arc` | `<cx> <cy> <r> <start> <sweep>` |
| `polygon` | `<cx> <cy> <r> <sides>` |
| `ellipse` | `<cx> <cy> <rx> <ry>` |
| `spline` | `<x1 y1 x2 y2 ...>` |
| `slot` | `<x1> <y1> <x2> <y2> <w>` |
| `text` | `<str> [x] [y] [h]` |

### Features
| Command | Args |
|---------|------|
| `extrude` | `<dist> [op] [prof]` |
| `revolve` | `<angle> [prof] [axis] [op]` |
| `loft` | `<i1> <i2> ...` |
| `sweep` | `<profile> <path> [op]` |
| `helix` | `<pitch> <height>` |
| `pipe` | `<path> <size> [wall]` |
| `hole` | `<body> <face> <x> <y> <dia> <depth>` |
| `shell` | `<body> <thick> [faces...]` |
| `fillet` | `<body> <r> [edges...]` |
| `chamfer` | `<body> <d> [edges...]` |
| `mirror-body` | `<body> [plane]` |
| `pattern-body` | `<body> <xn> <xs>` |
| `circular` | `<body> <count> [axis]` |
| `combine` | `<target> <tools...> [op]` |
| `scale` | `<body> <sx> [sy] [sz]` |
| `move` | `<body> <dx> <dy> <dz>` |
| `rotate` | `<body> [axis] [angle]` |
| `press-pull` | `<body> <face> <dist>` |
| `thicken` | `<thickness>` |
| `draft` | `<body> <face> <angle>` |
| `thread` | `<body> <face>` |

### Assembly
| Command | Args |
|---------|------|
| `component` | `[name]` |
| `move-to-comp` | `<body> <comp>` |
| `joint` | `<c1> <c2> <type>` |
| `as-built` | `<c1> <c2> <type>` |

### Body
| Command | Args |
|---------|------|
| `delete` | `[body]` |
| `rename` | `<body> <name>` |
| `copy` | `[body] [name] [dx dy dz]` |
| `visibility` | `[body]` |

### Construction
| Command | Args |
|---------|------|
| `plane` | `[base] [offset]` |
| `axis` | `[type] [body]` |

### Parameters
| Command | Args |
|---------|------|
| `params` | |
| `add-param` | `<name> <val> [unit]` |
| `set-param` | `<name> <val>` |

### Appearance
| Command | Args |
|---------|------|
| `appearances` | `[search]` |
| `apply` | `<body> <appearance>` |
| `color` | `<body> <r> <g> <b> [a]` |

### Export
| Command | Args |
|---------|------|
| `stl` | `[path]` |
| `step` | `[path]` |
| `export-3mf` | `[path]` |
| `f3d` | `[path]` |
| `screenshot` | `[path] [w] [h]` |

### History
| Command | Args |
|---------|------|
| `undo` | `[steps]` |
| `redo` | `[steps]` |
| `save` | `[desc]` |
| `save-as` | `<name> [desc]` |
| `script` | `<code>` |

## Measurement Units
All linear dimensions are in **centimeters (cm)**.
Angles are in **degrees**.
