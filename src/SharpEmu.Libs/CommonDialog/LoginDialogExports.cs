// Copyright (C) 2026 SharpEmu Emulator Project
// SPDX-License-Identifier: GPL-2.0-or-later

using SharpEmu.HLE;

namespace SharpEmu.Libs.CommonDialog;

public static class LoginDialogExports
{
    [SysAbiExport(
        Nid = "qP-EvQRl2Hc",
        ExportName = "sceLoginDialogInitialize",
        Target = Generation.Gen4 | Generation.Gen5,
        LibraryName = "libSceLoginDialog")]
    public static int LoginDialogInitialize(CpuContext ctx)
    {
        ctx[CpuRegister.Rax] = 0;
        return (int)OrbisGen2Result.ORBIS_GEN2_OK;
    }
}
