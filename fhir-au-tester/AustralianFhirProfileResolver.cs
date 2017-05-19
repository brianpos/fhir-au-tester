using Hl7.Fhir.Model;
using Hl7.Fhir.Specification.Source;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace fhir_au_tester
{
    public class AustralianFhirProfileResolver : IResourceResolver
    {
        public List<StructureDefinition> TestProfiles = new List<StructureDefinition>();

        public AustralianFhirProfileResolver()
        {
            // TestProfiles.Add();
        }
        public Resource ResolveByCanonicalUri(string uri)
        {
            return TestProfiles.SingleOrDefault(p => p.Url == uri);
        }

        public Resource ResolveByUri(string uri)
        {
            return ResolveByCanonicalUri(uri);
        }
    }
}
