﻿using System.Collections.Generic;
using System.Linq;
using OurMemory.Data.Infrastructure;
using OurMemory.Domain.Entities;
using OurMemory.Service.Interfaces;

namespace OurMemory.Service
{
    public class ImageVeteranService : IImageVeteranService
    {
        private readonly IRepository<ImageVeteran> _imageVeteran;
        private readonly IUnitOfWork _unitOfWork;

        public ImageVeteranService(IRepository<ImageVeteran> imageVeteran, IUnitOfWork unitOfWork)
        {
            _imageVeteran = imageVeteran;
            _unitOfWork = unitOfWork;
        }

        public void DeleteImagesVeteran(IEnumerable<ImageVeteran> imageVeterans)
        {
            var veterans = imageVeterans.ToList();

            for (int i = 0; i < veterans.Count(); i++)
            {
                _imageVeteran.Delete(veterans[i]);
            }

            Save();
        }

        public void Save()
        {
            _unitOfWork.Commit();
        }
    }
}