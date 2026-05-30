using System.Text.Json;

namespace FusionCli;

static class CommandDispatcher
{
    public static async Task<int> Run(string[] args)
    {
        if (args.Length == 0) { PrintHelp(); return 0; }

        var cmd = args[0].ToLowerInvariant();
        var a = args.Length > 1 ? args[1..] : [];

        using var client = new FusionClient();

        // Switch on command — each case maps directly to Fusion HTTP bridge
        switch (cmd)
        {
            // ---- Status & Info ----
            case "status":   await client.PingAndPrintAsync(); break;
            case "info":     await client.CallAndPrintAsync("get_info"); break;
            case "bodies":   await client.CallAndPrintAsync("get_bodies_info"); break;
            case "faces":    await client.CallAndPrintAsync("get_face_info", P(["body", S(a, 0, "0")])); break;
            case "edges":    await client.CallAndPrintAsync("get_edge_info", P(["body", S(a, 0, "0")])); break;
            case "sketch-info": await client.CallAndPrintAsync("get_sketch_info", P(["sketch", S(a, 0, "0")])); break;
            case "timeline": await client.CallAndPrintAsync("get_timeline_info"); break;
            case "measure":  await client.CallAndPrintAsync("measure_body", P(["body", S(a, 0, "0")])); break;
            case "distance":
                if (a.Length < 2) { Console.WriteLine("Usage: distance <entity1> <entity2>"); return 1; }
                await client.CallAndPrintAsync("measure_between", P(["entity1", a[0], "entity2", a[1]]));
                break;

            // ---- Document ----
            case "new-doc":  await client.CallAndPrintAsync("create_new_document", P(["name", S(a, 0, "Untitled")])); break;
            case "clear":    await client.CallAndPrintAsync("clear_design"); break;

            // ---- Sketch ----
            case "sketch":
                await client.CallAndPrintAsync("create_sketch", P(["plane", S(a, 0, "XY"), "name", S(a, 1, "")]));
                break;
            case "sketch-face":
                await client.CallAndPrintAsync("create_sketch_on_face", P(["body", S(a, 0, "0"), "face_index", I(a, 1, 0), "name", S(a, 2, "")]));
                break;
            case "finish-sketch":
                await client.CallAndPrintAsync("finish_sketch", P(["sketch", S(a, 0, "")]));
                break;
            case "delete-sketch":
                await client.CallAndPrintAsync("delete_sketch", P(["sketch", S(a, 0, "0")]));
                break;
            case "rect":
                await client.CallAndPrintAsync("draw_rectangle", P(["x1", D(a,0,0), "y1", D(a,1,0), "x2", D(a,2,10), "y2", D(a,3,10)]));
                break;
            case "center-rect":
                await client.CallAndPrintAsync("draw_center_rectangle", P(["cx", D(a,0,0), "cy", D(a,1,0), "width", D(a,2,10), "height", D(a,3,10)]));
                break;
            case "circle":
                await client.CallAndPrintAsync("draw_circle", P(["cx", D(a,0,0), "cy", D(a,1,0), "radius", D(a,2,5)]));
                break;
            case "line":
                await client.CallAndPrintAsync("draw_line", P(["x1", D(a,0,0), "y1", D(a,1,0), "x2", D(a,2,10), "y2", D(a,3,0)]));
                break;
            case "arc":
                await client.CallAndPrintAsync("draw_arc", P(["cx", D(a,0,0), "cy", D(a,1,0), "radius", D(a,2,5), "start_angle", D(a,3,0), "sweep_angle", D(a,4,90)]));
                break;
            case "polygon":
                await client.CallAndPrintAsync("draw_polygon", P(["cx", D(a,0,0), "cy", D(a,1,0), "radius", D(a,2,5), "sides", I(a,3,6)]));
                break;
            case "ellipse":
                await client.CallAndPrintAsync("draw_ellipse", P(["cx", D(a,0,0), "cy", D(a,1,0), "rx", D(a,2,5), "ry", D(a,3,3)]));
                break;
            case "spline":
                var pts = ParsePointPairs(a);
                await client.CallAndPrintAsync("draw_spline", P(["points", pts]));
                break;
            case "slot":
                await client.CallAndPrintAsync("draw_slot", P(["x1", D(a,0,0), "y1", D(a,1,0), "x2", D(a,2,10), "y2", D(a,3,0), "width", D(a,4,3)]));
                break;
            case "text":
                if (a.Length < 1) { Console.WriteLine("Usage: text <str> [x] [y] [height]"); return 1; }
                await client.CallAndPrintAsync("draw_text", P(["text", a[0], "x", D(a,1,0), "y", D(a,2,0), "height", D(a,3,1.0)]));
                break;
            case "sketch-fillet":
                await client.CallAndPrintAsync("add_sketch_fillet", P(["line1_index", I(a,0,0), "line2_index", I(a,1,1), "radius", D(a,2,1.0)]));
                break;
            case "offset":
                await client.CallAndPrintAsync("offset_sketch", P(["distance", D(a,0,1.0), "dx", D(a,1,1), "dy", D(a,2,1)]));
                break;
            case "mirror":
                await client.CallAndPrintAsync("mirror_sketch", P(["axis_line_index", I(a,0,0)]));
                break;
            case "pattern":
                await client.CallAndPrintAsync("rectangular_pattern_sketch", P(["x_count", I(a,0,2), "y_count", I(a,1,2), "x_spacing", D(a,2,5), "y_spacing", D(a,3,5)]));
                break;

            // ---- Constraints ----
            case "constraint":
                if (a.Length < 1) { Console.WriteLine("Usage: constraint <type> [e1_idx e2_idx] [types]"); return 1; }
                await client.CallAndPrintAsync("add_constraint", P(["constraint_type", a[0], "entity1_index", I(a,1,0), "entity2_index", I(a,2,-1)]));
                break;
            case "dimension":
                if (a.Length < 2) { Console.WriteLine("Usage: dimension <type> <value> [e1_idx e2_idx]"); return 1; }
                await client.CallAndPrintAsync("add_sketch_dimension", P(["dimension_type", a[0], "value", D(a,1,5.0), "entity1_index", I(a,2,0), "entity2_index", I(a,3,-1)]));
                break;

            // ---- Features ----
            case "extrude":
                await client.CallAndPrintAsync("extrude", P(["distance", D(a,0,1.0), "operation", S(a,1,"new_body"), "profile_index", I(a,2,0)]));
                break;
            case "revolve":
                await client.CallAndPrintAsync("revolve", P(["angle", D(a,0,360), "profile_index", I(a,1,0), "axis_index", I(a,2,0), "operation", S(a,3,"new_body")]));
                break;
            case "loft":
                if (a.Length < 2) { Console.WriteLine("Usage: loft <idx1> <idx2> [idx3...]"); return 1; }
                await client.CallAndPrintAsync("loft", P(["sketch_indices", a.Select(int.Parse).ToList()]));
                break;
            case "sweep":
                await client.CallAndPrintAsync("sweep", P(["profile_sketch_index", I(a,0,0), "path_sketch_index", I(a,1,1), "operation", S(a,2,"new_body")]));
                break;
            case "helix":
                await client.CallAndPrintAsync("helix", P(["pitch", D(a,0,1.0), "height", D(a,1,5.0)]));
                break;
            case "pipe":
                await client.CallAndPrintAsync("create_pipe", P(["path_sketch_index", I(a,0,0), "section_size", D(a,1,0.5), "wall_thickness", D(a,2,0)]));
                break;
            case "hole":
                if (a.Length < 6) { Console.WriteLine("Usage: hole <body> <face> <x> <y> <dia> <depth>"); return 1; }
                await client.CallAndPrintAsync("create_hole", P(["body", a[0], "face_index", int.Parse(a[1]), "x", double.Parse(a[2]), "y", double.Parse(a[3]), "diameter", double.Parse(a[4]), "depth", double.Parse(a[5])]));
                break;
            case "shell":
                if (a.Length < 2) { Console.WriteLine("Usage: shell <body> <thickness> [face_indices...]"); return 1; }
                await client.CallAndPrintAsync("shell", P(["body", a[0], "thickness", double.Parse(a[1]), "face_indices", a[2..].Select(int.Parse).ToList()]));
                break;
            case "fillet":
                if (a.Length < 2) { Console.WriteLine("Usage: fillet <body> <radius> [edge_indices...]"); return 1; }
                await client.CallAndPrintAsync("fillet", P(["body", a[0], "radius", double.Parse(a[1]), "edge_indices", a[2..].Length > 0 ? a[2..].Select(int.Parse).ToList() : [0]]));
                break;
            case "chamfer":
                if (a.Length < 2) { Console.WriteLine("Usage: chamfer <body> <distance> [edge_indices...]"); return 1; }
                await client.CallAndPrintAsync("chamfer", P(["body", a[0], "distance", double.Parse(a[1]), "edge_indices", a[2..].Length > 0 ? a[2..].Select(int.Parse).ToList() : [0]]));
                break;
            case "mirror-body":
                await client.CallAndPrintAsync("mirror_body", P(["body", S(a,0,"0"), "plane", S(a,1,"XY")]));
                break;
            case "pattern-body":
                await client.CallAndPrintAsync("rectangular_pattern_body", P(["body", S(a,0,"0"), "x_count", I(a,1,2), "x_spacing", D(a,2,5)]));
                break;
            case "circular":
                await client.CallAndPrintAsync("circular_pattern_body", P(["body", S(a,0,"0"), "count", I(a,1,4), "axis", S(a,2,"Z")]));
                break;
            case "combine":
                if (a.Length < 3) { Console.WriteLine("Usage: combine <target> <tool1> [tool2...] [operation]"); return 1; }
                var comOp = a[^1] is "join" or "cut" or "intersect" ? a[^1] : "join";
                var comTools = a[1..].Where(x => x is not ("join" or "cut" or "intersect")).ToList();
                await client.CallAndPrintAsync("combine_bodies", P(["target_body", a[0], "tool_bodies", comTools, "operation", comOp]));
                break;
            case "scale":
                await client.CallAndPrintAsync("scale_body", P(["body", S(a,0,"0"), "scale_x", D(a,1,2.0)]));
                break;
            case "move":
                if (a.Length < 4) { Console.WriteLine("Usage: move <body> <dx> <dy> <dz>"); return 1; }
                await client.CallAndPrintAsync("move_body", P(["body", a[0], "dx", double.Parse(a[1]), "dy", double.Parse(a[2]), "dz", double.Parse(a[3])]));
                break;
            case "rotate":
                await client.CallAndPrintAsync("rotate_body", P(["body", S(a,0,"0"), "axis", S(a,1,"Z"), "angle", D(a,2,45)]));
                break;
            case "press-pull":
                if (a.Length < 3) { Console.WriteLine("Usage: press-pull <body> <face> <distance>"); return 1; }
                await client.CallAndPrintAsync("press_pull", P(["body", a[0], "face_index", int.Parse(a[1]), "distance", double.Parse(a[2])]));
                break;
            case "thicken":
                await client.CallAndPrintAsync("thicken", P(["thickness", D(a,0,0.5)]));
                break;
            case "draft":
                await client.CallAndPrintAsync("draft_face", P(["body", S(a,0,"0"), "face_index", I(a,1,0), "angle", D(a,2,3)]));
                break;
            case "thread":
                await client.CallAndPrintAsync("add_thread", P(["body", S(a,0,"0"), "face_index", I(a,1,0)]));
                break;

            // ---- Assembly ----
            case "component":
                await client.CallAndPrintAsync("create_component", P(["name", S(a,0,"New Component")]));
                break;
            case "move-to-comp":
                if (a.Length < 2) { Console.WriteLine("Usage: move-to-comp <body> <component>"); return 1; }
                await client.CallAndPrintAsync("move_body_to_component", P(["body", a[0], "component", a[1]]));
                break;
            case "joint":
                if (a.Length < 3) { Console.WriteLine("Usage: joint <comp1> <comp2> <type>"); return 1; }
                await client.CallAndPrintAsync("create_joint", P(["component1", a[0], "component2", a[1], "joint_type", a[2]]));
                break;
            case "as-built":
                if (a.Length < 3) { Console.WriteLine("Usage: as-built <comp1> <comp2> <type>"); return 1; }
                await client.CallAndPrintAsync("create_as_built_joint", P(["component1", a[0], "component2", a[1], "joint_type", a[2]]));
                break;

            // ---- Body Management ----
            case "delete":
                await client.CallAndPrintAsync("delete_body", P(["body", S(a,0,"0")]));
                break;
            case "rename":
                if (a.Length < 2) { Console.WriteLine("Usage: rename <body> <name>"); return 1; }
                await client.CallAndPrintAsync("rename_body", P(["body", a[0], "name", a[1]]));
                break;
            case "copy":
                await client.CallAndPrintAsync("copy_body", P(["body", S(a,0,"0"), "name", S(a,1,""), "dx", D(a,2,2), "dy", D(a,3,0), "dz", D(a,4,0)]));
                break;
            case "visibility":
                await client.CallAndPrintAsync("toggle_body_visibility", P(["body", S(a,0,"0")]));
                break;

            // ---- Construction ----
            case "plane":
                await client.CallAndPrintAsync("add_construction_plane", P(["base_plane", S(a,0,"XY"), "offset", D(a,1,2.0)]));
                break;
            case "axis":
                await client.CallAndPrintAsync("add_construction_axis", P(["axis_type", S(a,0,"edge"), "body", S(a,1,"0")]));
                break;

            // ---- Parameters ----
            case "params":
                await client.CallAndPrintAsync("list_parameters");
                break;
            case "add-param":
                if (a.Length < 2) { Console.WriteLine("Usage: add-param <name> <value> [unit]"); return 1; }
                await client.CallAndPrintAsync("add_parameter", P(["name", a[0], "value", double.Parse(a[1]), "unit", S(a,2,"cm")]));
                break;
            case "set-param":
                if (a.Length < 2) { Console.WriteLine("Usage: set-param <name> <value>"); return 1; }
                await client.CallAndPrintAsync("update_parameter", P(["name", a[0], "value", double.Parse(a[1])]));
                break;

            // ---- Appearance ----
            case "appearances":
                await client.CallAndPrintAsync("list_appearances", P(["search", S(a,0,"")]));
                break;
            case "apply":
                if (a.Length < 2) { Console.WriteLine("Usage: apply <body> <appearance>"); return 1; }
                await client.CallAndPrintAsync("apply_appearance", P(["body", a[0], "appearance", a[1]]));
                break;
            case "color":
                if (a.Length < 4) { Console.WriteLine("Usage: color <body> <r> <g> <b> [opacity]"); return 1; }
                await client.CallAndPrintAsync("set_body_color", P(["body", a[0], "r", int.Parse(a[1]), "g", int.Parse(a[2]), "b", int.Parse(a[3]), "opacity", I(a,4,255)]));
                break;

            // ---- Export ----
            case "stl":         await client.CallAndPrintAsync("export_stl", a.Length > 0 ? P(["path", a[0]]) : null); break;
            case "step":        await client.CallAndPrintAsync("export_step", a.Length > 0 ? P(["path", a[0]]) : null); break;
            case "export-3mf":  await client.CallAndPrintAsync("export_3mf", a.Length > 0 ? P(["path", a[0]]) : null); break;
            case "f3d":         await client.CallAndPrintAsync("export_f3d", a.Length > 0 ? P(["path", a[0]]) : null); break;
            case "screenshot":
                await client.CallAndPrintAsync("capture_screenshot", P(["path", S(a,0,""), "width", I(a,1,1920), "height", I(a,2,1080)]));
                break;

            // ---- History ----
            case "undo":    await client.CallAndPrintAsync("undo", P(["steps", I(a,0,1)])); break;
            case "redo":    await client.CallAndPrintAsync("redo", P(["steps", I(a,0,1)])); break;
            case "save":    await client.CallAndPrintAsync("save", P(["description", S(a,0,"Saved")])); break;
            case "save-as":
                if (a.Length < 1) { Console.WriteLine("Usage: save-as <name> [description]"); return 1; }
                await client.CallAndPrintAsync("save_as", P(["name", a[0], "description", S(a,1,"Saved")]));
                break;
            case "script":
            case "exec":
                if (a.Length < 1) { Console.WriteLine("Usage: script <python_code>"); return 1; }
                await client.CallAndPrintAsync("execute_script", P(["code", string.Join(" ", a)]));
                break;

            case "help":
            case "--help":
            case "-h":
                PrintHelp();
                break;

            case "install":
                return RunInstall(a);

            default:
                Console.WriteLine($"Unknown command: {cmd}");
                Console.WriteLine("Run 'fusion help' for available commands.");
                return 1;
        }
        return Environment.ExitCode;
    }

    // Helper: build params dictionary from key-value pairs
    static Dictionary<string, object?> P(object?[] pairs)
    {
        var d = new Dictionary<string, object?>();
        for (int i = 0; i + 1 < pairs.Length; i += 2)
        {
            var key = pairs[i]?.ToString();
            if (key != null) d[key] = pairs[i + 1];
        }
        return d;
    }

    static string S(string[] a, int idx, string def) => idx < a.Length ? a[idx] : def;
    static int I(string[] a, int idx, int def) => idx < a.Length && int.TryParse(a[idx], out var v) ? v : def;
    static double D(string[] a, int idx, double def) => idx < a.Length && double.TryParse(a[idx], out var v) ? v : def;

    static List<List<double>> ParsePointPairs(string[] a)
    {
        var pts = new List<List<double>>();
        for (int i = 0; i + 1 < a.Length; i += 2)
            if (double.TryParse(a[i], out var x) && double.TryParse(a[i + 1], out var y))
                pts.Add([x, y]);
        return pts.Count > 0 ? pts : [[0, 0], [5, 5], [10, 0]];
    }

    static void PrintHelp()
    {
        Console.WriteLine("""
            Fusion 360 CLI - Control Fusion 360 from the command line

            Usage: fusion <command> [args]

            Setup:
              install [source]                 Install FusionMCP add-in to Fusion 360
                                               source: path to fusion-mcp directory (default: ../fusion-mcp)

            Status & Info:
              status                          Check connection
              info                            Full design info
              bodies                          List all bodies
              faces [body]                    List body faces
              edges [body]                    List body edges
              sketch-info [sketch]            Sketch details
              timeline                        Feature timeline
              measure [body]                  Measure body
              distance <e1> <e2>              Measure distance

            Document:
              new-doc [name]                  New document
              clear                           Clear design

            Sketch:
              sketch [plane] [name]           Create sketch (XY/XZ/YZ)
              sketch-face <body> <face>       Sketch on face
              finish-sketch [sketch]          Finish sketch
              delete-sketch [sketch]          Delete sketch
              rect <x1> <y1> <x2> <y2>        Rectangle
              center-rect <cx> <cy> <w> <h>   Center rectangle
              circle <cx> <cy> <r>            Circle
              line <x1> <y1> <x2> <y2>        Line
              arc <cx> <cy> <r> <start> <sw>  Arc
              polygon <cx> <cy> <r> <sides>   Polygon
              ellipse <cx> <cy> <rx> <ry>     Ellipse
              spline <x1 y1 x2 y2 ...>        Spline
              slot <x1> <y1> <x2> <y2> <w>    Slot
              text <str> [x] [y] [h]          Text
              sketch-fillet <l1> <l2> <r>     Sketch fillet
              offset <dist> <dx> <dy>         Offset
              mirror <axis>                   Mirror sketch
              pattern <xn> <yn> <xs> <ys>     Pattern

            Constraints:
              constraint <type> [e1 e2]       Add constraint
              dimension <type> <val> [e1 e2]  Add dimension

            Features:
              extrude <dist> [op] [prof]      Extrude
              revolve <angle> [prof] [axis]   Revolve
              loft <i1> <i2> ...              Loft
              sweep <profile> <path> [op]     Sweep
              helix <pitch> <height>          Helix
              pipe <path> <size> [wall]       Pipe
              hole <body> <face> <x> <y> <d> <depth>  Hole
              shell <body> <thick> [faces...] Shell
              fillet <body> <r> [edges...]    Fillet
              chamfer <body> <d> [edges...]   Chamfer
              mirror-body <body> [plane]      Mirror body
              pattern-body <body> <xn> <xs>   Pattern body
              circular <body> <count> [axis]  Circular pattern
              combine <target> <tools...> [op] Boolean combine
              scale <body> <sx> [sy] [sz]     Scale
              move <body> <dx> <dy> <dz>      Move
              rotate <body> [axis] [angle]    Rotate
              press-pull <body> <face> <dist> Press/pull
              thicken <thickness>             Thicken
              draft <body> <face> <angle>     Draft
              thread <body> <face>            Add thread

            Assembly:
              component [name]                New component
              move-to-comp <body> <comp>      Move to component
              joint <c1> <c2> <type>          Create joint
              as-built <c1> <c2> <type>       As-built joint

            Body:
              delete [body]                   Delete body
              rename <body> <name>            Rename body
              copy [body] [name] [dx dy dz]   Copy body
              visibility [body]               Toggle visibility

            Construction:
              plane [base] [offset]           Construction plane
              axis [type] [body]              Construction axis

            Parameters:
              params                          List parameters
              add-param <name> <val> [unit]   Add parameter
              set-param <name> <val>          Update parameter

            Appearance:
              appearances [search]            List appearances
              apply <body> <appearance>        Apply appearance
              color <body> <r> <g> <b> [a]    Set color

            Export:
              stl [path]                      Export STL
              step [path]                     Export STEP
              export-3mf [path]               Export 3MF
              f3d [path]                      Export F3D
              screenshot [path] [w] [h]       Screenshot

            History:
              undo [steps]                    Undo
              redo [steps]                    Redo
              save [desc]                     Save
              save-as <name> [desc]           Save as
              script <code>                   Execute script
            """);
    }

    static int RunInstall(string[] a)
    {
        // Find fusion-mcp source directory
        var exeDir = AppDomain.CurrentDomain.BaseDirectory;
        var defaultSource = Path.GetFullPath(Path.Combine(exeDir, "..", "..", "..", "..", "fusion-mcp"));
        var source = a.Length > 0 ? a[0] : defaultSource;

        if (!Directory.Exists(source))
        {
            // Try resolving relative to current working directory
            source = Path.GetFullPath(source);
            if (!Directory.Exists(source))
            {
                Console.WriteLine($"Error: fusion-mcp directory not found at: {source}");
                Console.WriteLine();
                Console.WriteLine("Usage: fusion install [path-to-fusion-mcp]");
                Console.WriteLine("  Example: fusion install ../fusion-mcp");
                return 1;
            }
        }

        var srcFile = Path.Combine(source, "FusionMCP.py");
        var srcManifest = Path.Combine(source, "FusionMCP.manifest");
        if (!File.Exists(srcFile))
        {
            Console.WriteLine($"Error: FusionMCP.py not found in: {source}");
            return 1;
        }

        // Determine Fusion 360 add-ins directory
        var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        string[] candidateDirs =
        [
            Path.Combine(home, "Library", "Application Support", "Autodesk", "Autodesk Fusion 360", "API", "AddIns"),
            Path.Combine(home, "AppData", "Roaming", "Autodesk", "Autodesk Fusion 360", "API", "AddIns"),
        ];

        string? addinsDir = null;
        foreach (var dir in candidateDirs)
        {
            if (Directory.Exists(dir)) { addinsDir = dir; break; }
        }

        if (addinsDir == null)
        {
            Console.WriteLine("Error: Could not find Fusion 360 add-ins directory.");
            Console.WriteLine("Searched:");
            foreach (var dir in candidateDirs)
                Console.WriteLine($"  {dir}");
            Console.WriteLine();
            Console.WriteLine("Please specify manually:");
            Console.WriteLine("  Open Fusion 360 → press Shift+S → go to ADD-INS tab → note the add-ins folder path");
            return 1;
        }

        var destDir = Path.Combine(addinsDir, "FusionMCP");
        Directory.CreateDirectory(destDir);

        // Copy files
        var filesToCopy = new[] { "FusionMCP.py", "FusionMCP.manifest" };
        foreach (var file in filesToCopy)
        {
            var src = Path.Combine(source, file);
            if (File.Exists(src))
            {
                File.Copy(src, Path.Combine(destDir, file), overwrite: true);
                Console.WriteLine($"  Copied: {file}");
            }
        }

        Console.WriteLine();
        Console.WriteLine($"Installed to: {destDir}");
        Console.WriteLine();
        Console.WriteLine("Next steps:");
        Console.WriteLine("  1. Open Autodesk Fusion 360");
        Console.WriteLine("  2. Press Shift+S to open the Scripts and Add-Ins panel");
        Console.WriteLine("  3. Switch to the Add-Ins tab");
        Console.WriteLine("  4. Click the '+' (Add) button and select the FusionMCP folder");
        Console.WriteLine("  5. Select FusionMCP from the list and click Run");
        Console.WriteLine("  6. You should see a popup: 'FusionMCP bridge is running on port 7432'");
        Console.WriteLine();
        Console.WriteLine("Verify: fusion status");
        return 0;
    }
}
