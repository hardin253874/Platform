// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Collections.Generic;
using EDC.IO;
using EDC.ReadiNow.IO;
using EDC.ReadiNow.Model;
using Moq;
using NUnit.Framework;

namespace EDC.ReadiNow.Test.IO
{
    [TestFixture]
    [RunWithTransaction]
    public class CleanupFileRepositoriesActionTests
    {
        private IEnumerable<string> CreateTestPhotos(int count)
        {
            var photos = new List<PhotoFileType>();
            ISet<string> hashes = new HashSet<string>();

            for (var i = 0; i < count; i++)
            {
                var hash = Guid.NewGuid().ToString();

                var photo = new PhotoFileType {FileDataHash = hash};
                photos.Add(photo);

                hashes.Add(hash);
            }

            Entity.Save(photos);

            return hashes;
        }


        [Test]
        public void FailedDeleteUnreferencedTokensAreCleanedTest()
        {
            IEnumerable<string> unreferencedTokens1 = new List<string> { Guid.NewGuid().ToString(), Guid.NewGuid().ToString() };
            IEnumerable<string> unreferencedTokens2 = new List<string> { Guid.NewGuid().ToString(), Guid.NewGuid().ToString() };

            var mockRepo = new MockRepository(MockBehavior.Strict);

            var fileRepo1Mock = mockRepo.Create<IFileRepository>();
            fileRepo1Mock.Setup(r => r.GetTokens()).Returns(unreferencedTokens1);
            // Deleting from this repo will throw exceptions
            fileRepo1Mock.Setup(s => s.Delete(It.IsIn(unreferencedTokens1))).Throws<Exception>();

            // This repo should still delete files even though exceptions were thrown
            var fileRepo2Mock = mockRepo.Create<IFileRepository>();
            fileRepo2Mock.Setup(r => r.GetTokens()).Returns(unreferencedTokens2);
            fileRepo2Mock.Setup(s => s.Delete(It.IsIn(unreferencedTokens2)));

            var action = new CleanupFileRepositoriesAction(new List<IFileRepository> {fileRepo1Mock.Object, fileRepo2Mock.Object}, 1000);
            action.Execute(null);

            mockRepo.VerifyAll();
        }


        [Test]
        public void NoTokensTest()
        {
            IEnumerable<string> unreferencedTokens1 = new List<string>();
            IEnumerable<string> unreferencedTokens2 = new List<string>();

            var mockRepo = new MockRepository(MockBehavior.Strict);

            var fileRepo1Mock = mockRepo.Create<IFileRepository>();
            fileRepo1Mock.Setup(r => r.GetTokens()).Returns(unreferencedTokens1);

            var fileRepo2Mock = mockRepo.Create<IFileRepository>();
            fileRepo2Mock.Setup(r => r.GetTokens()).Returns(unreferencedTokens2);

            var action = new CleanupFileRepositoriesAction(new List<IFileRepository> {fileRepo1Mock.Object, fileRepo2Mock.Object}, 1000);
            action.Execute(null);

            mockRepo.VerifyAll();
        }


        [Test]
        [RunAsDefaultTenant]
        [TestCase(1000)]
        [TestCase(2)]
        public void OnlyUnreferencedTokensCleanedTest(int batchSize)
        {
            var referencedHashes1 = CreateTestPhotos(2);
            IEnumerable<string> unreferencedHashes1 = new List<string> {Guid.NewGuid().ToString(), Guid.NewGuid().ToString()};
            var referencedHashes2 = CreateTestPhotos(2);
            IEnumerable<string> unreferencedHashes2 = new List<string> {Guid.NewGuid().ToString(), Guid.NewGuid().ToString()};

            var hashes1 = new List<string>();
            hashes1.AddRange(referencedHashes1);
            hashes1.AddRange(unreferencedHashes1);

            var hashes2 = new List<string>();
            hashes2.AddRange(referencedHashes2);
            hashes2.AddRange(unreferencedHashes2);

            var mockRepo = new MockRepository(MockBehavior.Strict);

            var fileRepo1Mock = mockRepo.Create<IFileRepository>();
            fileRepo1Mock.Setup(r => r.GetTokens()).Returns(hashes1);
            fileRepo1Mock.Setup(s => s.Delete(It.IsIn(unreferencedHashes1)));

            var fileRepo2Mock = mockRepo.Create<IFileRepository>();
            fileRepo2Mock.Setup(r => r.GetTokens()).Returns(hashes2);
            fileRepo2Mock.Setup(s => s.Delete(It.IsIn(unreferencedHashes2)));

            var action = new CleanupFileRepositoriesAction(new List<IFileRepository> {fileRepo1Mock.Object, fileRepo2Mock.Object}, batchSize);
            action.Execute(null);

            mockRepo.VerifyAll();
        }


        [Test]
        [RunAsDefaultTenant]
        public void ReferencedTokensNotCleanedTest()
        {
            var referencedHashes1 = CreateTestPhotos(2);
            var referencedHashes2 = CreateTestPhotos(2);

            var mockRepo = new MockRepository(MockBehavior.Strict);

            var fileRepo1Mock = mockRepo.Create<IFileRepository>();
            fileRepo1Mock.Setup(r => r.GetTokens()).Returns(referencedHashes1);

            var fileRepo2Mock = mockRepo.Create<IFileRepository>();
            fileRepo2Mock.Setup(r => r.GetTokens()).Returns(referencedHashes2);

            var action = new CleanupFileRepositoriesAction(new List<IFileRepository> {fileRepo1Mock.Object, fileRepo2Mock.Object}, 1000);
            action.Execute(null);

            mockRepo.VerifyAll();
        }


        [Test]
        public void UnreferencedTokensAreCleanedTest()
        {
            IEnumerable<string> unreferencedTokens1 = new List<string> {Guid.NewGuid().ToString(), Guid.NewGuid().ToString()};
            IEnumerable<string> unreferencedTokens2 = new List<string> {Guid.NewGuid().ToString(), Guid.NewGuid().ToString()};

            var mockRepo = new MockRepository(MockBehavior.Strict);

            var fileRepo1Mock = mockRepo.Create<IFileRepository>();
            fileRepo1Mock.Setup(r => r.GetTokens()).Returns(unreferencedTokens1);
            fileRepo1Mock.Setup(s => s.Delete(It.IsIn(unreferencedTokens1)));

            var fileRepo2Mock = mockRepo.Create<IFileRepository>();
            fileRepo2Mock.Setup(r => r.GetTokens()).Returns(unreferencedTokens2);
            fileRepo2Mock.Setup(s => s.Delete(It.IsIn(unreferencedTokens2)));

            var action = new CleanupFileRepositoriesAction(new List<IFileRepository> {fileRepo1Mock.Object, fileRepo2Mock.Object}, 1000);
            action.Execute(null);

            mockRepo.VerifyAll();
        }
    }
}