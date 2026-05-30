---
name: fusion360-designer
description: Use this skill when the user wants to design, model, or create 3D parts in Fusion 360. This skill should be used when users mention Fusion 360, 3D modeling, CAD, designing parts, creating geometry, or ask to build physical objects. The skill drives the Fusion 360 CLI to create sketches, extrude, revolve, shell, fillet, pattern, and export designs.
---

# Fusion 360 Designer

This skill enables Claude to design and model 3D parts in Autodesk Fusion 360 through the `fusion` CLI tool. The CLI communicates with the FusionMCP add-in running inside Fusion 360 via HTTP on localhost:7432.

## Prerequisites

1. Autodesk Fusion 360 is open with the FusionMCP add-in running
2. The `fusion` CLI is built and available (or use `dotnet run --` from the FusionCli directory)
3. Verify connection before starting: `fusion status`

All dimensions are in **centimeters (cm)** unless otherwise noted.

## Core Workflow

When designing a part, follow this general pattern:

1. **Verify connection** → `fusion status`
2. **Check existing state** → `fusion info` / `fusion bodies` / `fusion timeline`
3. **Clear if needed** → `fusion clear`
4. **Create sketch on a plane** → `fusion sketch XY <name>`
5. **Draw 2D geometry** → `fusion circle`, `fusion rect`, `fusion line`, etc.
6. **Finish sketch** → `fusion finish-sketch`
7. **Apply 3D feature** → `fusion extrude`, `fusion revolve`, `fusion loft`, etc.
8. **Add details** → `fusion fillet`, `fusion chamfer`, `fusion shell`, `fusion hole`
9. **Verify result** → `fusion bodies` / `fusion measure` / `fusion screenshot`
10. **Export if requested** → `fusion stl` / `fusion step`

## Command Reference

### Sketch creation
```bash
fusion sketch XY <name>           # Floor plane
fusion sketch XZ <name>           # Front plane
fusion sketch YZ <name>           # Side plane
fusion sketch-face <body> <face>  # On body face
```

### 2D drawing
```bash
fusion circle <cx> <cy> <r>
fusion rect <x1> <y1> <x2> <y2>
fusion center-rect <cx> <cy> <w> <h>
fusion line <x1> <y1> <x2> <y2>
fusion arc <cx> <cy> <r> <start> <sweep>
fusion polygon <cx> <cy> <r> <sides>
fusion ellipse <cx> <cy> <rx> <ry>
fusion spline <x1 y1 x2 y2 ...>
fusion slot <x1> <y1> <x2> <y2> <w>
fusion text <str> [x] [y] [h]
```

### 3D features
```bash
fusion extrude <dist> [new_body|join|cut] [profile_index]
fusion extrude-to <target_body> <target_face> [profile_index]
fusion extrude-all [profile_index] [operation]
fusion revolve <angle> [profile_index] [axis_index] [operation]
fusion loft <idx1> <idx2> ...
fusion sweep <profile_idx> <path_idx> [operation]
fusion shell <body> <thickness> [face_indices...]
fusion fillet <body> <radius> [edge_indices...]
fusion variable-fillet <body> <start_radius> <end_radius> [edge_indices...]
fusion chamfer <body> <distance> [edge_indices...]
fusion hole <body> <face> <x> <y> <dia> <depth> [--type simple|counterbore|countersink]
fusion mirror-body <body> [plane]
fusion pattern-body <body> <x_count> <x_spacing>
fusion circular <body> <count> [axis]
fusion combine <target> <tools...> [join|cut|intersect]
fusion scale <body> <sx> [sy] [sz]
fusion move <body> <dx> <dy> <dz>
fusion rotate <body> [axis] [angle]
fusion press-pull <body> <face> <distance>
fusion thicken <thickness>
fusion draft <body> <face> <angle>
fusion thread <body> <face>
fusion emboss <body> <depth>
fusion rib <thickness> [direction]
fusion web <thickness>
fusion split-face <body> <face>
fusion split-body <body> <cutting_body>
```

### Surface modeling
```bash
fusion patch [profile_index]
fusion stitch <body1> [body2...]
fusion trim-surface <body> <cutting_body>
fusion extend-surface <body> <face> <distance>
fusion offset-surface <body> <distance> [face_indices...]
fusion delete-face <body> <face_indices...>
fusion replace-face <body> <face> <replacement_body>
fusion thicken-surface <body> <thickness>
```

### Import
```bash
fusion import-step <path>
fusion import-mesh <path>  (STL/OBJ/3MF)
fusion import-dxf <path> [sketch]
```

### Info & inspection
```bash
fusion status              # Check connection
fusion info               # Full design info
fusion bodies             # List all bodies
fusion faces <body>       # List body faces (find indices for fillet/shell/hole)
fusion edges <body>       # List body edges (find indices for fillet/chamfer)
fusion sketch-info <idx>  # Sketch details
fusion timeline           # Feature history
fusion measure <body>     # Dimensions & volume
fusion screenshot [path]  # Capture viewport image
```

### Export & file
```bash
fusion stl [path]         # Export STL
fusion step [path]        # Export STEP
fusion export-3mf [path]  # Export 3MF
fusion f3d [path]         # Export F3D
fusion undo [steps]       # Undo last features
fusion save [desc]        # Save design
fusion save-as <name>     # Save as new
fusion script "<code>"    # Execute Python in Fusion for complex operations
```

## Design Patterns

### Hollow box / container
```bash
fusion sketch XY box
fusion center-rect 0 0 10 6
fusion finish-sketch
fusion extrude 4
# Find top face index via: fusion faces 0
fusion shell 0 0.3 <top_face_index>
```

### Hole / cutout in a face
When extrude-cut fails with "no target body", use `fusion script` with negative distance:
```bash
fusion script "
import adsk.core, adsk.fusion
design = adsk.fusion.Design.cast(app.activeProduct)
root = design.rootComponent
sketch = root.sketches.item(<index>)
profile = sketch.profiles.item(<profile_index>)
ext_input = root.features.extrudeFeatures.createInput(profile, adsk.fusion.FeatureOperations.CutFeatureOperation)
ext_input.setDistanceExtent(False, adsk.core.ValueInput.createByReal(-1.0))
root.features.extrudeFeatures.add(ext_input)
result['output'] = 'Cut done'
"
```

### Multi-step operations
Chain commands logically. Always check `fusion faces` or `fusion edges` to get correct indices before fillet, chamfer, shell, or hole operations. After each major step, verify with `fusion bodies` or `fusion timeline`.

## Important Notes

- After `fusion sketch-face`, the sketch coordinate system aligns with that face
- `fusion extrude` uses the last created/finished sketch by default
- Use `fusion script` for operations that the standard commands don't cover
- After modifying the FusionMCP plugin, Fusion 360 must be fully restarted (not just the add-in)
- The `install` command deploys the plugin: `fusion install [path-to-fusion-mcp]`
