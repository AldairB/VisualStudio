﻿using System.Reactive.Linq;
using System.Threading.Tasks;
using NSubstitute;
using NUnit.Framework;
using UnitTests;
using GitHub.Services;
using System.Linq.Expressions;
using System;
using GitHub.Models;

public class RepositoryCloneServiceTests
{
    public class TheCloneRepositoryMethod : TestBaseClass
    {
        [Test]
        [Ignore("LocalRepositoryModel sucks. It does a Directory.Exists, making this test fail")]
        public async Task ClonesToRepositoryPathAsync()
        {
            var serviceProvider = Substitutes.ServiceProvider;
            var operatingSystem = serviceProvider.GetOperatingSystem();
            var vsGitServices = serviceProvider.GetVSGitServices();
            var cloneService = serviceProvider.GetRepositoryCloneService();

            await cloneService.CloneRepository("https://github.com/foo/bar", "bar", @"c:\dev");

            operatingSystem.Directory.Received().CreateDirectory(@"c:\dev\bar");
            await vsGitServices.Received().Clone("https://github.com/foo/bar", @"c:\dev\bar", true);
        }

        [Test]
        [Ignore("LocalRepositoryModel sucks. It does a Directory.Exists, making this test fail")]
        public async Task UpdatesMetricsWhenRepositoryClonedAsync()
        {
            var serviceProvider = Substitutes.ServiceProvider;
            var operatingSystem = serviceProvider.GetOperatingSystem();
            var gitSerivce = serviceProvider.GetGitService();
            var vsGitServices = serviceProvider.GetVSGitServices();
            var usageTracker = Substitute.For<IUsageTracker>();
            var cloneService = new RepositoryCloneService(operatingSystem, gitSerivce, vsGitServices, usageTracker);

            await cloneService.CloneRepository("https://github.com/foo/bar", "bar", @"c:\dev");
            var model = UsageModel.Create(Guid.NewGuid());

            await usageTracker.Received().IncrementCounter(
                Arg.Is<Expression<Func<UsageModel.MeasuresModel, int>>>(x => 
                    ((MemberExpression)x.Body).Member.Name == nameof(model.Measures.NumberOfClones)));
        }
    }
}
