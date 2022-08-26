using System.Collections.Generic;
using Xunit;

namespace Veldrid.SPIRV.Tests
{
    public class ReflectionTests
    {
        private void AssertEqual(ResourceLayoutElementDescription a, ResourceLayoutElementDescription b)
        {
            Assert.Equal(a.Name, b.Name);
            Assert.Equal(a.Kind, b.Kind);
            Assert.Equal(a.Options, b.Options);
            Assert.Equal(a.Stages, b.Stages);
        }

        public static IEnumerable<object[]> ShaderSetsAndResources()
        {
            yield return new object[]
            {
                "planet.vert.spv",
                "planet.frag.spv",
                new VertexElementDescription[]
                {
                    new VertexElementDescription("Position", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float3),
                    new VertexElementDescription("Normal", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float3),
                    new VertexElementDescription("TexCoord", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float2),
                },
                new ResourceLayoutDescription[]
                {
                    new ResourceLayoutDescription(
                        new ResourceLayoutElementDescription("vdspv_0_0", ResourceKind.UniformBuffer, ShaderStages.Vertex | ShaderStages.Fragment),
                        UnusedResource,
                        new ResourceLayoutElementDescription("vdspv_0_2", ResourceKind.UniformBuffer, ShaderStages.Fragment)),
                    new ResourceLayoutDescription(
                        new ResourceLayoutElementDescription("vdspv_1_0", ResourceKind.TextureReadOnly, ShaderStages.Fragment),
                        new ResourceLayoutElementDescription("vdspv_1_1", ResourceKind.Sampler, ShaderStages.Fragment))
                }
            };
        }

        private static readonly ResourceLayoutElementDescription UnusedResource
            = new() { Options = (ResourceLayoutElementOptions)2 };
    }
}
