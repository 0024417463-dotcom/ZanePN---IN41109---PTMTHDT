using System.ComponentModel.DataAnnotations;
using EduHub.Application.Clubs;
using EduHub.Domain.Entities;
using EduHub.Domain.Interfaces;
using FluentAssertions;
using Moq;

namespace EduHub.Tests.Clubs;

public class CreateClubCommandTests
{
    private readonly Mock<IClubRepository> _repo = new();

    [Fact]
    public async Task CreateClub_WithValidName_SavesAndReturnsClub()
    {
        var handler = new CreateClubCommandHandler(_repo.Object);

        var club = await handler.HandleAsync(new CreateClubCommand("  CLB Lập trình  ", "mô tả"));

        club.Name.Should().Be("CLB Lập trình");
        _repo.Verify(r => r.AddAsync(club, default), Times.Once);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task CreateClub_WhenNameEmpty_ThrowsValidationException(string name)
    {
        var handler = new CreateClubCommandHandler(_repo.Object);

        var act = () => handler.HandleAsync(new CreateClubCommand(name, null));

        await act.Should().ThrowAsync<ValidationException>();
        _repo.Verify(r => r.AddAsync(It.IsAny<Club>(), default), Times.Never);
    }
}
