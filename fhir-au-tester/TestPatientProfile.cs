using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Hl7.Fhir.Model;
using Hl7.Fhir.Specification.Source;
using Hl7.Fhir.Validation;
using Hl7.Fhir.Rest;
using Hl7.Fhir.Specification.Terminology;

namespace fhir_au_tester
{
    [TestClass]
    public class TestPatientProfile
    {
        [TestInitialize]
        public void SetupSource()
        {
            // Ensure the FHIR extensions are registered
            Hl7.Fhir.FhirPath.PocoNavigatorExtensions.PrepareFhirSymbolTableFunctions();

            // Prepare the artifact resolvers (cache to reduce complexity)
            _source = new CachedResolver(
                new MultiResolver(
                    // Use the specification zip that is local (from the NuGet package)
                    new ZipSource("specification.zip"),
                    // Try using the fixed content
                    new AustralianFhirProfileResolver(),
                    // Then look to a specific FHIR Registry Server
                    new WebResolver(id => new FhirClient("http://sqlonfhir-stu3.azurewebsites.net/fhir"))
                    )
                );

            var ctx = new ValidationSettings()
            {
                ResourceResolver = _source,
                GenerateSnapshot = true,
                EnableXsdValidation = true,
                Trace = false,
                ResolveExteralReferences = true
            };

            // until we have a local terminology service ready, here is the remote implementation
            // ctx.TerminologyService = new ExternalTerminologyService(new FhirClient("http://test.fhir.org/r3"));
            ctx.TerminologyService = new LocalTerminologyServer(_source);

            _validator = new Validator(ctx);
        }

        IResourceResolver _source;
        Validator _validator;

        [TestMethod]
        public void CheckSimplePatient()
        {
            var p = new Practitioner();
            p.Name.Add(new HumanName().WithGiven("Brian").AndFamily("Postlethwaite"));
            p.BirthDateElement = new Date(1970, 1, 1);
            var hpi_i = new Identifier("http://ns.electronichealth.net.au/id/hi/hpii/1.0", "8003610833334085");
            hpi_i.Type = new CodeableConcept("http://hl7.org/fhir/v2/0203", "NPI", "National provider identifier", "HPI-I");
            p.Identifier.Add(hpi_i);


            var ctx = new ValidationSettings() { ResourceResolver = _source, GenerateSnapshot = true, ResolveExteralReferences = true, Trace = false };

            _validator = new Validator(ctx);

            var report = _validator.Validate(p, new[] { "http://sqlonfhir-stu3.azurewebsites.net/fhir/StructureDefinition/0ac0bb0d1d864cee965f401f2654fbb0" });
            System.Diagnostics.Trace.WriteLine(report.ToString());
            Assert.IsTrue(report.Success);
            Assert.AreEqual(1, report.Warnings);            // 1 unresolvable reference
        }
    }
}
