﻿using BitTools.Core.Contracts;
using BitTools.Core.Model;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace BitCodeGenerator.Implementations
{
    public class DefaultProjectDtoRulesProvider : IProjectDtoRulesProvider
    {
        public virtual IList<DtoRules> GetProjectAllDtoRules(Project project)
        {
            if (project == null)
                throw new ArgumentNullException(nameof(project));

            IList<DtoRules> allDtoRules = new List<DtoRules>();

            foreach (Document doc in project.Documents)
            {
                if (!doc.SupportsSemanticModel)
                    continue;

                SemanticModel semanticModel = doc.GetSemanticModelAsync().Result;

                SyntaxNode root = doc.GetSyntaxRootAsync(CancellationToken.None).Result;

                List<ClassDeclarationSyntax> allDtoRulesClasses = new List<ClassDeclarationSyntax>();

                foreach (ClassDeclarationSyntax classDeclarationSyntax in root.DescendantNodes()
                    .OfType<ClassDeclarationSyntax>())
                {
                    if (classDeclarationSyntax.BaseList == null)
                        continue;

                    bool isDtoRules = classDeclarationSyntax.BaseList.Types.Select(t => t.Type)
                        .Select(t => semanticModel.GetSymbolInfo(t).Symbol?.OriginalDefinition?.ToString())
                        .Any(t => t == "Foundation.Api.DtoRules.DtoRules<TDto>");

                    if (isDtoRules == true)
                    {
                        isDtoRules = semanticModel.GetDeclaredSymbol(classDeclarationSyntax)
                            .GetAttributes()
                            .Any(att => att.AttributeClass.Name == "AutoGenerateAttribute");
                    }

                    if (isDtoRules == true)
                        allDtoRulesClasses.Add(classDeclarationSyntax);
                }

                if (!allDtoRulesClasses.Any())
                    continue;

                foreach (ClassDeclarationSyntax dtoRulesClassDec in allDtoRulesClasses)
                {
                    INamedTypeSymbol dtoRuleSymbol = (INamedTypeSymbol)semanticModel.GetDeclaredSymbol(dtoRulesClassDec);

                    DtoRules dtoRules = new DtoRules
                    {
                        DtoRulesSymbol = dtoRuleSymbol,
                        Name = dtoRuleSymbol.Name,
                        ClassDeclaration = dtoRulesClassDec,
                        DtoSymbol = (dtoRuleSymbol.BaseType.TypeArguments).ExtendedSingle($"Looking for dto of {dtoRuleSymbol.Name}"),
                        DtoRulesDocument = doc,
                        SemanticModel = semanticModel,
                        ClassSyntaxTree = dtoRulesClassDec.SyntaxTree
                    };

                    dtoRules.ClassRootNode = (CompilationUnitSyntax)dtoRules.ClassSyntaxTree.GetRoot();

                    allDtoRules.Add(dtoRules);
                }
            }

            return allDtoRules;
        }
    }
}