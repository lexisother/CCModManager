﻿using Mono.Cecil;
using Mono.Cecil.Cil;
using MonoMod.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Olympus {
    public unsafe class CmdAhornInstallAhorn : Cmd<IEnumerator> {
        public override bool LogRun => false;
        public override IEnumerator Run() {
            return Cmds.Get<CmdAhornRunJuliaTask>().Run(@"
env = ENV[""AHORN_ENV""]

logfilePath = joinpath(dirname(env), ""log-install-ahorn.txt"")
println(""Logging to "" * logfilePath)
logfile = open(logfilePath, ""w"")

flush(stdout)
flush(stderr)

stdoutReal = stdout
(rd, wr) = redirect_stdout()
redirect_stderr(stdout)

@async while !eof(rd)
    data = String(readavailable(rd))
    print(stdoutReal, data)
    flush(stdoutReal)
    print(logfile, data)
    flush(logfile)
end


if VERSION < v""1.3""
    println(""Outdated version of Julia - $VERSION installed, 1.3+ needed."")
    exit(1)
end

using Logging
logger = SimpleLogger(stdout, Logging.Debug)
loggerPrev = Logging.global_logger(logger)

using Pkg
Pkg.activate(ENV[""AHORN_ENV""])

install_or_update(url::String, pkg::String) = if ""Ahorn"" ∈ keys(Pkg.Types.Context().env.project.deps)
    println(""Updating $pkg..."")
    Pkg.update(pkg)
else
    println(""Adding $pkg..."")
    Pkg.add(PackageSpec(url = url))
end

try
    println(""#OLYMPUS# TIMEOUT START"")

    Pkg.instantiate()

    install_or_update(""https://github.com/CelestialCartographers/Maple.git"", ""Maple"")
    install_or_update(""https://github.com/CelestialCartographers/Ahorn.git"", ""Ahorn"")

    Pkg.instantiate()
    Pkg.API.precompile()

    import Ahorn

    println(""#OLYMPUS# TIMEOUT END"")
catch e
    println(""FATAL ERROR"")
    println(sprint(showerror, e, catch_backtrace()))
    exit(1)
end

exit(0)
", null);
        }
    }
}
