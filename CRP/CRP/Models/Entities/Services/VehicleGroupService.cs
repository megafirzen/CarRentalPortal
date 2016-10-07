﻿using CRP.Models.Entities.Repositories;
using CRP.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CRP.Models.Entities.Services
{
    public class VehicleGroupService
    {

        VehicleGroupRepository _repository = new VehicleGroupRepository();
        PriceGroupRepository _pRepository = new PriceGroupRepository();

        public List<VehicleGroup> getAll()
        {
            List<VehicleGroup> listVehicleGroup = new List<VehicleGroup>();
            listVehicleGroup = _repository.getList();
            return listVehicleGroup;
        }

        public bool add(VehicleGroup entity)
        {
            try
            {
                _pRepository.Add(entity.PriceGroup);
                _repository.Add(entity);
            }
            catch(Exception e)
            {
                Console.WriteLine(e);
                return false;
            }
            return true;
        }

        public bool update(VehicleGroup entity)
        {
            try
            {
                _repository.Update(entity);
            }
            catch(Exception e)
            {
                Console.WriteLine(e);
                return false;
            }
            return true;
        }

        public bool delete(VehicleGroup entity)
        {
            try
            {
                _repository.Delete(entity);
            }
            catch(Exception e)
            {
                Console.WriteLine(e);
                return false;
            }
            return true;
        }

        public VehicleGroup findByID(int id)
        {
            return _repository.findById(id);
        }
    }
}