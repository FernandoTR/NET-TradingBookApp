using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces;

public interface IStringUtilitiesService
{
    string GetRandomString(int length);
    string GetRandomPassword(int length);
    string GetRandomHash();
    string GetHash(HashAlgorithm hashAlgorithm, string input);
}
