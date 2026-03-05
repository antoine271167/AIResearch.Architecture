using AIResearch.Architecture.Application.Models;

namespace AIResearch.Architecture.Application.Services.Interfaces;

internal interface ICodeGeneratorService
{
    string GenerateCode(CodeGenerationModel model);
}
