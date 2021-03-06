﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace StackExchange.Redis.Tests
{
    [Collection(SharedConnectionFixture.Key)]
    public class Roles : TestBase
    {
        public Roles(ITestOutputHelper output, SharedConnectionFixture fixture) : base(output, fixture) { }

        [Fact]
        public void MasterRole()
        {
            using var muxer = Create(allowAdmin: true);
            var server = muxer.GetServer(TestConfig.Current.MasterServerAndPort);

            var role = server.Role();
            Assert.NotNull(role);
            Assert.Equal(role.Value, RedisLiterals.master);
            var master = role as Role.Master;
            Assert.NotNull(master);
            Assert.NotNull(master.Replicas);
            Assert.Contains(master.Replicas, r =>
                r.Ip == TestConfig.Current.ReplicaServer &&
                r.Port == TestConfig.Current.ReplicaPort);
        }

        [Fact]
        public void ReplicaRole()
        {
            var connString = $"{TestConfig.Current.ReplicaServerAndPort},allowAdmin=true";
            using var muxer = ConnectionMultiplexer.Connect(connString);
            var server = muxer.GetServer(TestConfig.Current.ReplicaServerAndPort);

            var role = server.Role();
            Assert.NotNull(role);
            var replica = role as Role.Replica;
            Assert.NotNull(replica);
            Assert.Equal(replica.MasterIp, TestConfig.Current.MasterServer);
            Assert.Equal(replica.MasterPort, TestConfig.Current.MasterPort);
        }

        [Fact]
        public void RoleRequiresAdmin()
        {
            using var muxer = Create(allowAdmin: false);
            var server = muxer.GetServer(TestConfig.Current.MasterServerAndPort);

            Assert.Throws<RedisCommandException>(() => server.Role());
        }
    }
}
