// veldrid-spirv.cpp : Defines the entry point for the console application.
//

#include "libveldrid-spirv.hpp"
#include "InteropStructs.hpp"
#include <fstream>
#include <map>
#include <sstream>
#include "shaderc.hpp"
#include <iostream>

using namespace spirv_cross;

namespace Veldrid
{
CompilationResult *CompileGLSLToSPIRV(
    const std::string sourceText,
    shaderc_shader_kind kind,
    std::string fileName,
    const shaderc::CompileOptions &options)
{
    shaderc::Compiler compiler;
    shaderc::SpvCompilationResult result = compiler.CompileGlslToSpv(sourceText, kind, fileName.c_str(), options);

    if (result.GetCompilationStatus() != shaderc_compilation_status_success)
    {
        return new CompilationResult(result.GetErrorMessage());
    }

    uint32_t length = static_cast<uint32_t>(result.end() - result.begin()) * sizeof(uint32_t);
    CompilationResult *ret = new CompilationResult();
    ret->Succeeded = 1;
    ret->DataBuffers.Resize(1);
    ret->DataBuffers[0].CopyFrom(length, (uint8_t *)result.begin());
    return ret;
}

VD_EXPORT CompilationResult *CompileGlslToSpirv(GlslCompileInfo *info)
{
    try
    {
        shaderc::CompileOptions options;

        if (info->Debug)
        {
            options.SetGenerateDebugInfo();
        }
        else
        {
            options.SetOptimizationLevel(shaderc_optimization_level_performance);
        }

        for (uint32_t i = 0; i < info->Macros.Count; i++)
        {
            const MacroDefinition &macro = info->Macros[i];
            if (macro.ValueLength == 0)
            {
                options.AddMacroDefinition(std::string(macro.Name, macro.NameLength));
            }
            else
            {
                options.AddMacroDefinition(
                    std::string(macro.Name, macro.NameLength),
                    std::string(macro.Value, macro.ValueLength));
            }
        }

        return CompileGLSLToSPIRV(
            std::string(info->SourceText.Data, info->SourceText.Count),
            info->Kind,
            std::string(info->FileName.Data, info->FileName.Count),
            options);
    }
    catch (std::exception e)
    {
        return new CompilationResult(e.what());
    }
}

VD_EXPORT void FreeResult(CompilationResult *result)
{
    delete result;
}
} // namespace Veldrid
