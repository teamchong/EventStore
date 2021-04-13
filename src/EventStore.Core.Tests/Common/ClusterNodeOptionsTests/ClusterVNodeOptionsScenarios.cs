using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using EventStore.Core.Authentication;
using EventStore.Core.Authentication.InternalAuthentication;
using EventStore.Core.Authorization;
using EventStore.Core.LogAbstraction;
using EventStore.Core.Tests.Services.Transport.Tcp;
using NUnit.Framework;

namespace EventStore.Core.Tests.Common.ClusterNodeOptionsTests {
	[TestFixture]
	public abstract class SingleNodeScenario {
		protected ClusterVNode _node;
		protected ClusterVNodeOptions _options;

		[OneTimeSetUp]
		public virtual void TestFixtureSetUp() {
			_options = WithOptions(
				new ClusterVNodeOptions()
					.RunInMemory()
					.Secure(new X509Certificate2Collection(ssl_connections.GetRootCertificate()),
						ssl_connections.GetServerCertificate()));
			_node = new ClusterVNode<string>(_options, LogFormatAbstractor.V2,
				new AuthenticationProviderFactory(c => new InternalAuthenticationProviderFactory(c)),
				new AuthorizationProviderFactory(c => new LegacyAuthorizationProviderFactory(c.MainQueue)));
			_node.Start();
		}

		[OneTimeTearDown]
		public virtual Task TestFixtureTearDown() => _node?.StopAsync() ?? Task.CompletedTask;

		protected abstract ClusterVNodeOptions WithOptions(ClusterVNodeOptions options);

	}

	[TestFixture, Category("LongRunning")]
	public abstract class ClusterMemberScenario {
		protected ClusterVNode _node;
		protected int _clusterSize = 3;
		protected int _quorumSize;
		protected ClusterVNodeOptions _options;

		[OneTimeSetUp]
		public virtual void TestFixtureSetUp() {
			_quorumSize = _clusterSize / 2 + 1;

			_options = WithOptions(new ClusterVNodeOptions()
				.InCluster(_clusterSize)
				.RunInMemory()
				.Secure(new X509Certificate2Collection(ssl_connections.GetRootCertificate()),
					ssl_connections.GetServerCertificate()));
			_node = new ClusterVNode<string>(_options, LogFormatAbstractor.V2,
				new AuthenticationProviderFactory(_ => new InternalAuthenticationProviderFactory(_)),
				new AuthorizationProviderFactory(c => new LegacyAuthorizationProviderFactory(c.MainQueue)));
			_node.Start();
		}

		[OneTimeTearDown]
		public virtual Task TestFixtureTearDown() => _node?.StopAsync() ?? Task.CompletedTask;

		protected abstract ClusterVNodeOptions WithOptions(ClusterVNodeOptions options);
	}
}
