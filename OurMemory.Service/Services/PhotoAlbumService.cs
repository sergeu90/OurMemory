﻿using System.Collections.Generic;
using System.Linq;
using OurMemory.Data.Infrastructure;
using OurMemory.Domain.Entities;
using OurMemory.Service.Interfaces;
using OurMemory.Service.Model;
using OurMemory.Service.Specification;
using OurMemory.Data.Specification.Core;

namespace OurMemory.Service.Services
{
    public class PhotoAlbumService : IPhotoAlbumService
    {
        private readonly IRepository<PhotoAlbum> _photoAlbumRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly PhotoAlbumSpecification _photoAlbumSpecification;

        public PhotoAlbumService(IRepository<PhotoAlbum> photoAlbumRepository, IUnitOfWork unitOfWork, PhotoAlbumSpecification photoAlbumSpecification)
        {
            _photoAlbumRepository = photoAlbumRepository;
            _unitOfWork = unitOfWork;
            _photoAlbumSpecification = photoAlbumSpecification;
        }

        public void Add(PhotoAlbum photoAlbum)
        {
            _photoAlbumRepository.Add(photoAlbum);
            SavePhotoAlbum();
        }

        public IEnumerable<PhotoAlbum> GetAll(bool withImages = false)
        {
            if (withImages)
            {
                var photoAlbums = _photoAlbumRepository
                    .GetAll()
                    .Where(x => !x.IsDeleted)
                    .Select(x=> new PhotoAlbum()
                    {
                        User = x.User,
                        Images = null,
                        Id = x.Id,
                        Image = x.Image,
                        Description = x.Description,
                        CreatedDateTime = x.CreatedDateTime,
                        Title = x.Title,
                        Views = x.Views,
                        IsDeleted = x.IsDeleted,
                        UpdatedDateTime = x.UpdatedDateTime
                    });
              
            }

            return _photoAlbumRepository.GetAll().Where(x => !x.IsDeleted);
        }

        public PhotoAlbum GetById(int id)
        {
            PhotoAlbum veteran = _photoAlbumRepository.GetById(id);

            return veteran.IsDeleted == false ? veteran : null;
        }

        public List<Image> GetPhotoAlbums(int idAlbum)
        {
            var collection = _photoAlbumRepository.GetById(idAlbum).Images.Where(x => !x.IsDeleted).ToList();

            return collection;
        }

        public void UpdatePhotoAlbum(PhotoAlbum photoAlbum)
        {
            _photoAlbumRepository.Update(photoAlbum);
            SavePhotoAlbum();
        }

        public IQueryable<PhotoAlbum> SearchPhotoAlbum(SearchPhotoAlbumModel searchVeteranModel)
        {
            Specification<PhotoAlbum> keyWord = _photoAlbumSpecification.KeyWord(searchVeteranModel);

            var photoAlbums = _photoAlbumRepository.GetSpec(keyWord.Predicate).OrderBy(x => x.Title);
            return photoAlbums;
        }

        public void SavePhotoAlbum()
        {
            _unitOfWork.Commit();
        }
    }
}