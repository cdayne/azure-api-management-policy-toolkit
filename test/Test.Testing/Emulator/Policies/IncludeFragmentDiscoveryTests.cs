// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Azure.ApiManagement.PolicyToolkit.Authoring;
using Microsoft.Azure.ApiManagement.PolicyToolkit.Testing;
using Microsoft.Azure.ApiManagement.PolicyToolkit.Testing.Document;

namespace Test.Emulator.Emulator.Policies;

// The emulator finds a fragment by the name the compiler gives the document: the [Document] name, or the class name
// when the attribute doesn't give one. The document type comes from the attribute or the implemented interface, and
// ids are matched the way the fragment registry matches them.
[TestClass]
public class IncludeFragmentDiscoveryTests
{
    [Document("fragment-with-type", Type = DocumentType.Fragment)]
    public class FragmentWithType : IFragment
    {
        public void Fragment(IFragmentContext context) => context.SetVariable("ran", "with-type");
    }

    [Document("fragment-without-type")]
    public class FragmentWithoutType : IFragment
    {
        public void Fragment(IFragmentContext context) => context.SetVariable("ran", "without-type");
    }

    [Document]
    public class FragmentNamedByItsClass : IFragment
    {
        public void Fragment(IFragmentContext context) => context.SetVariable("ran", "named-by-class");
    }

    [Document("Mixed-Case-Fragment")]
    public class MixedCaseFragment : IFragment
    {
        public void Fragment(IFragmentContext context) => context.SetVariable("ran", "mixed-case");
    }

    class IncludingDocument(string fragmentId) : IDocument
    {
        public void Inbound(IInboundContext context) => context.IncludeFragment(fragmentId);
    }

    [TestMethod]
    [DataRow("fragment-with-type", "with-type")]
    [DataRow("fragment-without-type", "without-type")]
    [DataRow("FragmentNamedByItsClass", "named-by-class")]
    [DataRow("Mixed-Case-Fragment", "mixed-case")]
    [DataRow("mixed-case-fragment", "mixed-case")]
    public void ShouldRunFragmentFoundByTheNameTheCompilerGivesIt(string fragmentId, string expected)
    {
        var test = new IncludingDocument(fragmentId).AsTestDocument();

        test.RunInbound();

        test.Context.Variables.Should().ContainKey("ran").WhoseValue.Should().Be(expected);
    }
}
