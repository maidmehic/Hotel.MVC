﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hotel.Data.Models;
using Hotel.Web.Helper;
using Hotel.Web.Areas.ModulRecepcija.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace Hotel.Web.Areas.ModulRecepcija.Controllers
{
    [Area("ModulRecepcija")]
    public class CheckINController : Controller
    {
        MojContext db = new MojContext();
        public IActionResult Index(string PretraziPo,string pretraga)
        {
            CheckINIndexVM model = new CheckINIndexVM();
            model.Brojac = 0;
            if (PretraziPo == "Ime")
            {

                model.CheckINI = db.CheckIN.Where(x=>x.Gost.Ime.StartsWith(pretraga)).Select(x => new CheckINIndexVM.Row
                {
                    Id = x.Id,
                    BrojDjece = x.BrojDjece,
                    BrojOdraslih = x.BrojOdraslih,
                    DatumDolaska = x.DatumDolaska,
                    DatumOdlaska = x.DatumOdlaska,
                    Depozit = x.Depozit,
                    Napomena = x.Napomena,
                    Gost = x.Gost.Ime + " " + x.Gost.Prezime,
                    GostId=x.GostId,
                    Zaposlenik = x.Zaposlenik.Ime + " " + x.Zaposlenik.Prezime,
                   
                    TipUsluge = x.TipUsluge.Naziv + " " + x.TipUsluge.Cijena + "KM"
                    


                }).ToList();
               
            }
          
            if (PretraziPo == "BrojPasosa")
            {
                model.CheckINI = db.CheckIN.Where(x => x.Gost.BrojPasosa.StartsWith(pretraga)).Select(x => new CheckINIndexVM.Row
                {
                    Id = x.Id,
                    BrojDjece = x.BrojDjece,
                    BrojOdraslih = x.BrojOdraslih,
                    DatumDolaska = x.DatumDolaska,
                    DatumOdlaska = x.DatumOdlaska,
                    Depozit = x.Depozit,
                    Napomena = x.Napomena,
                    Gost = x.Gost.Ime + " " + x.Gost.Prezime,
                    GostId = x.GostId,
                    Zaposlenik = x.Zaposlenik.Ime + " " + x.Zaposlenik.Prezime,
                    TipUsluge = x.TipUsluge.Naziv + " " + x.TipUsluge.Cijena + "KM"



                }).ToList();
            }
            if (PretraziPo == null)
            {
                model.CheckINI = db.CheckIN.Select(x => new CheckINIndexVM.Row
                {
                    Id = x.Id,
                    BrojDjece = x.BrojDjece,
                    BrojOdraslih = x.BrojOdraslih,
                    DatumDolaska = x.DatumDolaska,
                    DatumOdlaska = x.DatumOdlaska,
                    Depozit = x.Depozit,
                    Napomena = x.Napomena,
                    Gost = x.Gost.Ime + " " + x.Gost.Prezime,
                    GostId = x.GostId,
                    Zaposlenik = x.Zaposlenik.Ime + " " + x.Zaposlenik.Prezime,
                    TipUsluge = x.TipUsluge.Naziv + " " + x.TipUsluge.Cijena + "KM"



                }).ToList();
            }
            if (model.CheckINI!=null)
                model.Brojac = model.CheckINI.Count;
            else
                model.Brojac = 0;
            return View(model);
        }
        public IActionResult Dodaj()
        {
            CheckINDodajVM model = new CheckINDodajVM();

            var gosti = db.Gost.Select(x => new
            {
                x.Id,
                Opis = "Ime i prezime: " + x.Ime + " " + x.Prezime + " Telefon: " + x.Telefon
            }).ToList();

            var Usluge = db.TipUsluge.Select(s => new
            {
            s.Id,
            Polje = string.Format("Naziv: {0} Cijena: {1}  ", s.Naziv, s.Cijena)
             }).ToList();

            model.Gosti = new SelectList(gosti, "Id", "Opis");
            model.TipoviUsluga = new SelectList(Usluge, "Id", "Polje");



            return View(model);
        }
        [HttpPost]
        public IActionResult Dodaj(CheckINDodajVM model)
        {
            CheckIN c = new CheckIN();

            c.GostId = model.Gost.Id;
            c.TipUslugeId = model.TipUsluge.Id;
            c.ZaposlenikId = HttpContext.GetLogiraniKorisnik().Id;// PREUZIMATI IZ SESIJE
            c.BrojDjece = model.BrojDjece;
            c.BrojOdraslih = model.BrojOdraslih;
            c.DatumDolaska = model.DatumDolaska;
            c.DatumOdlaska = model.DatumOdlaska;
            c.Depozit = model.Depozit;
            c.Napomena = model.Napomena;

            db.CheckIN.Add(c);
            db.SaveChanges();


            return RedirectToAction("Index");
        }
        public IActionResult CheckOut(int GostId)
        {
            CheckIN c = new CheckIN();
            c = db.CheckIN.Where(x => x.GostId == GostId).FirstOrDefault();
            c.DatumOdlaska = DateTime.Now.Date;

            // racunanje racuna i slanje u akciju dodajracun
            CheckINCheckOutVM model = new CheckINCheckOutVM();
            model.CheckInId = c.Id;

            double suma= new double();

            TipUsluge t = db.TipUsluge.Where(x => x.Id == c.TipUslugeId).FirstOrDefault();
            suma += t.Cijena;

            List<RezervisanaUsluga> rezervisane = db.RezervisanaUsluga.Include(x=>x.UslugeHotela).Where(x => x.CheckINId == c.Id).ToList();

            foreach (var i in rezervisane)
            {
                suma += i.UslugeHotela.Cijena;
            }

            List<RezervisanSmjestaj> smjestaji = db.RezervisanSmjestaj.Include(x=>x.Smjestaj).Where(x => x.CheckINId == c.Id).ToList();

            foreach (var I in smjestaji)
            {
                suma += I.Smjestaj.Cijena;
            }

            model.Iznos = suma;

            //

            //return RedirectToAction("Dodaj", "Feedback", new { c.GostId, c.Id });
            return View(model);
        }
        public IActionResult Detalji(int id)
        {
            CheckIN c = db.CheckIN.Where(x => x.Id == id).FirstOrDefault();

            

            return View(c);
        }
        public IActionResult Obrisi(int Id)
        {


           return RedirectToAction("Index");
        }
    }
}