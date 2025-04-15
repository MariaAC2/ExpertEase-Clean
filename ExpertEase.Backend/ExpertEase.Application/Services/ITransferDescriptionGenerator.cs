using ExpertEase.Domain.Entities;

namespace ExpertEase.Application.Services;

public interface ITransferDescriptionGenerator
{
    string Generate(Request request, Reply reply);
}