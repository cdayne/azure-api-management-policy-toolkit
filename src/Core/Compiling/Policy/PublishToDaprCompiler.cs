// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Xml.Linq;

using Microsoft.Azure.ApiManagement.PolicyToolkit.Authoring;
using Microsoft.Azure.ApiManagement.PolicyToolkit.Compiling.Diagnostics;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Microsoft.Azure.ApiManagement.PolicyToolkit.Compiling.Policy;

public class PublishToDaprCompiler : IMethodPolicyHandler
{
    public string MethodName => nameof(IInboundContext.PublishToDapr);

    public void Handle(IDocumentCompilationContext context, InvocationExpressionSyntax node)
    {
        if (!node.TryExtractingConfigParameter<PublishToDaprConfig>(context, "publish-to-dapr", out var values))
        {
            return;
        }

        var element = new XElement("publish-to-dapr");

        if (!element.AddAttribute(values, nameof(PublishToDaprConfig.Topic), "topic"))
        {
            context.Report(Diagnostic.Create(
                CompilationErrors.RequiredParameterNotDefined,
                node.GetLocation(),
                "publish-to-dapr",
                nameof(PublishToDaprConfig.Topic)
            ));
            return;
        }

        if (!values.TryGetValue(nameof(PublishToDaprConfig.Content), out var contentValue))
        {
            context.Report(Diagnostic.Create(
                CompilationErrors.RequiredParameterNotDefined,
                node.GetLocation(),
                "publish-to-dapr",
                nameof(PublishToDaprConfig.Content)
            ));
            return;
        }

        element.Value = contentValue.Value!;

        element.AddAttribute(values, nameof(PublishToDaprConfig.PubSubName), "pubsub-name");
        element.AddAttribute(values, nameof(PublishToDaprConfig.IgnoreError), "ignore-error");
        element.AddAttribute(values, nameof(PublishToDaprConfig.ResponseVariableName), "response-variable-name");
        element.AddAttribute(values, nameof(PublishToDaprConfig.Timeout), "timeout");
        element.AddAttribute(values, nameof(PublishToDaprConfig.Template), "template");
        element.AddAttribute(values, nameof(PublishToDaprConfig.ContentType), "content-type");

        context.AddPolicy(element);
    }
}