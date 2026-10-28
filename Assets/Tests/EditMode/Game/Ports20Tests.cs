// NIGHT20 (builder V): the TORCH's and the VORTEX's viewmodels (Game/Viewmodel.Ports20.cs, weaponModels.js torch() / vortex()):
// the muzzles are the web's ([0, 0.04, -0.46], [0, 0.05, -0.4] under the z flip); the parts split off the body add up to the
// whole model (the wall buy's hologram); the orb is three's IcosahedronGeometry(r, 1): 80 faces.
using NUnit.Framework;
using UnityEngine;

namespace Skygrind.Game.Tests
{
    public class Ports20Tests
    {
        [Test]
        public void TheMuzzlesAreTheWebs()
        {
            Assert.That(Vector3.Distance(HandsPortKit20.Torch(new MeshKit(), Color.red), new Vector3(0, 0.04f, 0.46f)), Is.LessThan(1e-6f));
            Assert.That(Vector3.Distance(HandsPortKit20.Vortex(new MeshKit(), Color.blue), new Vector3(0, 0.05f, 0.4f)), Is.LessThan(1e-6f));
        }

        [Test]
        public void ThePartsAddUpToTheWholeModel()
        {
            var whole = new MeshKit();
            HandsPortKit20.Torch(whole, Color.red);
            MeshKit body = new MeshKit(), pilot = new MeshKit(), nozzle = new MeshKit();
            HandsPortKit20.Torch(body, Color.red, pilot, nozzle);
            Assert.That(body.Vertices + pilot.Vertices + nozzle.Vertices, Is.EqualTo(whole.Vertices), "the TORCH");
            Assert.That(pilot.Vertices, Is.GreaterThan(0));
            Assert.That(nozzle.Vertices, Is.GreaterThan(0));
            var vWhole = new MeshKit();
            HandsPortKit20.Vortex(vWhole, Color.blue);
            MeshKit vBody = new MeshKit(), orb = new MeshKit();
            HandsPortKit20.Vortex(vBody, Color.blue, orb);
            Assert.That(vBody.Vertices + orb.Vertices, Is.EqualTo(vWhole.Vertices), "the VORTEX");
        }

        [Test]
        public void TheOrbIsAnIcosahedronOfDetailOne()
        {
            var a = new MeshKit();
            HandsPortKit20.Icosahedron1(a, 0.032f, Color.white);
            var b = new MeshKit();
            b.Triangle(Vector3.zero, Vector3.up, Vector3.right, Vector3.forward, Color.white);
            Assert.That(a.Vertices, Is.EqualTo(80 * b.Vertices), "20 faces x 4");
        }
    }
}
