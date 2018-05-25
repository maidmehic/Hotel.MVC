﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hotel.Data.Models;
using Hotel.Web.Areas.ModulRecepcija.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace Hotel.Web.Areas.ModulRecepcija.Controllers
{
    [Area("ModulRecepcija")]
    public class RezervisanSmjestajController : Controller
    {
        MojContext db = new MojContext();
        public IActionResult Index()
        {
            RezervisanSmjestajIndexVM model = new RezervisanSmjestajIndexVM();

            model.smjestaji = db.RezervisanSmjestaj.Select(x => new RezervisanSmjestajIndexVM.Row
            {
                Id=x.Id,
                Smjestaj = "Sprat: " + x.Smjestaj.Sprat + "," + x.Smjestaj.Kvadratura + "m^2, vrsta smjestaja: " + x.Smjestaj.VrstaSmjestaja.Naziv,
                Gost= x.Gost.Ime+" " +x.Gost.Prezime ,
                GostId=x.GostId,
                CheckINID=x.CheckINId,
                SmjestajId=x.SmjestajId,
                CheckIN= "Nosioc rezervacije: "+ x.CheckIN.Gost.Ime + " "+ x.CheckIN.Gost.Prezime +" Boravio od do" +x.CheckIN.DatumDolaska.ToShortDateString()+"-" +x.CheckIN.DatumOdlaska.ToShortDateString()
                
            }).ToList();



            return View(model);
        }
   //     public IActionResult Dodaj()
   //     {
   //         RezervisanSmjestajDodajVM model = new RezervisanSmjestajDodajVM(); // izmjenit vm , vracat ce se ko partial view na IndexOdabranogSmjestaja tu ce se birat gost koji boravi i msm da ce se trebat provjeravat kapacitet sobe

   //         var CheckINI =
   //         db.CheckIN
   //        .Select(s => new
   //        {
   //            s.Id,

   //            Polje = string.Format("Gost nosilac: {0} Datum Dolaska {1} Datum Odlaska: {2} /// Broj djece:{3} Broj odraslih: {4}", s.Gost.Ime + s.Gost.Prezime, s.DatumDolaska, s.DatumOdlaska, s.BrojDjece, s.BrojOdraslih)
   //        })
   // .ToList();

   //         var Smjestaji =
   //        db.Smjestaj
   //       .Where(x=>x.Zauzeto==false).Select(s => new
   //       {
   //           s.Id,

   //           Polje = string.Format("Vrsta smjestaja : {0} Sprat: {1}", s.VrstaSmjestaja.Naziv, s.Sprat)
   //       })
   //.ToList();

   //         var Gosti =
   //        db.Gost
   //       .Select(s => new
   //       {
   //           s.Id,

   //           Polje = string.Format("Gost: {0} Telefon: {1}", s.Ime + s.Prezime, s.Telefon)
   //       })
   //.ToList();

   //         model.Checkini = new SelectList(CheckINI, "Id", "Polje");
   //         model.Smjestaji = new SelectList(Smjestaji, "Id", "Polje");
   //         model.Gosti = new SelectList(Gosti, "Id", "Polje");

   //         return View(model);
   //     }
   //     [HttpPost]
   //     public IActionResult Dodaj(RezervisanSmjestajDodajVM model)
   //     {
   //         RezervisanSmjestaj s = new RezervisanSmjestaj();
   //         s.CheckINId = model.CheckIN.Id;
   //         s.GostId = model.Gost.Id;
   //         s.SmjestajId = model.Smjestaj.Id;
   //         ZauzmiSmjestaj(model.Smjestaj.Id);
   //         db.RezervisanSmjestaj.Add(s);
   //         db.SaveChanges();

   //         return RedirectToAction("Index");
   //     }
        public void ZauzmiSmjestaj(int id) {           
            Smjestaj s = new Smjestaj();
            s = db.Smjestaj.Where(x => x.Id == id).FirstOrDefault();
            s.Zauzeto = true;
            db.Smjestaj.Update(s);
        }
        public IActionResult Obrisi(int ID)
        {

            RezervisanSmjestaj s = new RezervisanSmjestaj();

            s = db.RezervisanSmjestaj.Include(x=>x.Smjestaj).Where(x => x.Id == ID).FirstOrDefault();
            s.Smjestaj.Zauzeto = false;
            db.Smjestaj.Update(s.Smjestaj);

            db.RezervisanSmjestaj.Remove(s);
            db.SaveChanges();

            return RedirectToAction("Index");
        }
        public IActionResult ProvjeriSlobodanSmjestaj()
        {
            RezervisanSmjestajProvjeriSlobodanSmjestajVM model = new RezervisanSmjestajProvjeriSlobodanSmjestajVM();

            model.lista = db.RezervisanSmjestaj.Where(x=>x.Smjestaj.Zauzeto==true).Include(x=>x.Smjestaj).Select(x => new RezervisanSmjestajProvjeriSlobodanSmjestajVM.Row
            {
               Gost=x.Gost.Ime + " "+ x.Gost.Prezime,
               Rezervisao= x.CheckIN.Gost.Ime + " " + x.CheckIN.Gost.Prezime,
               BoraviOdDo= "OD: "+x.CheckIN.DatumDolaska.ToShortDateString() +" DO: " + x.CheckIN.DatumOdlaska.ToShortDateString(),
               Zauzeto=x.Smjestaj.Zauzeto,
               RezervisanSmjestajId=x.Id,
               OpisSobe=x.Smjestaj.BrojSmjestaja.ToString() +" "+x.Smjestaj.VrstaSmjestaja.Naziv
            }).ToList();


           
          
            model.lista.AddRange(db.Smjestaj.Where(x => x.Zauzeto == false).Include(x=>x.VrstaSmjestaja).Select(x => new RezervisanSmjestajProvjeriSlobodanSmjestajVM.Row { Zauzeto = false, OpisSobe = x.BrojSmjestaja.ToString() + " " + x.VrstaSmjestaja.Naziv }).ToList());

            return View(model);
        }

        public IActionResult DodajGostaSobi(int SmjestajId,int CheckInId)
        {
            RezervisanSmjestajDodajGostaSobiVM model = new RezervisanSmjestajDodajGostaSobiVM();
            model.Gosti = db.Gost.Select(x => new SelectListItem {
                Text = x.Ime +" "+x.Prezime,
                Value = x.Id.ToString()

            }).ToList();
            model.CheckInId = CheckInId;
            model.SmjestajId = SmjestajId;
            return PartialView(model);
        }
        [HttpPost]
        public IActionResult DodajGostaSobi(RezervisanSmjestajDodajGostaSobiVM model)
        {

            //PROVJERA KAPACITETA   
            Smjestaj s = new Smjestaj();
            s = db.Smjestaj.Where(x => x.Id == model.SmjestajId).FirstOrDefault();

            List<RezervisanSmjestaj> lista = db.RezervisanSmjestaj.Where(x => x.SmjestajId == model.SmjestajId).ToList();

            if (lista.Count > (s.BrojKreveta + 1))
            {
                
               
                return RedirectToAction("IndexOdabranogSmjestaja", new { CheckINId = model.CheckInId, model.SmjestajId , poruka = "Nemoguće dodat gosta u smještaj , maximalni kapacitet dostignut" ,greska = true});
            }
           

            RezervisanSmjestaj ss = new RezervisanSmjestaj();

            ss.CheckINId = model.CheckInId;
            ss.GostId = model.GostId;
            ss.SmjestajId = model.SmjestajId;

            db.RezervisanSmjestaj.Add(ss);
            db.SaveChanges();


            return RedirectToAction("IndexOdabranogSmjestaja", new { CheckINId = model.CheckInId,  model.SmjestajId });
        }

        public IActionResult OdaberiSmjestaj(int CheckInId)
        {
            CheckIN c = new CheckIN();
            c = db.CheckIN.Where(x => x.Id == CheckInId).FirstOrDefault();

            var Smjestaji =
          db.Smjestaj
         .Where(x => x.Zauzeto == false).Select(s => new
         {
             s.Id,

             Polje = string.Format("Vrsta smjestaja : {0} Sprat: {1}", s.VrstaSmjestaja.Naziv, s.Sprat)
         })
  .ToList();
            RezervisanSmjestajOdaberiSmjestajVM model = new RezervisanSmjestajOdaberiSmjestajVM();

            model.DatumDolaska = c.DatumDolaska.ToShortDateString();
            model.DatumOdlaska = c.DatumOdlaska.ToShortDateString();
            model.Smjestaji = new SelectList(Smjestaji, "Id", "Polje");
            model.CheckInId = c.Id;
            model.GostId = c.GostId;

            return View(model);
        }
        [HttpPost]
        public IActionResult OdaberiSmjestaj(RezervisanSmjestajOdaberiSmjestajVM model)
        {

            RezervisanSmjestaj s = new RezervisanSmjestaj();
            s.CheckINId = model.CheckInId;
            s.GostId = model.GostId;
            s.SmjestajId = model.Smjestaj.Id;
            ZauzmiSmjestaj(model.Smjestaj.Id);
            db.RezervisanSmjestaj.Add(s);
            db.SaveChanges();



            return RedirectToAction("IndexOdabranogSmjestaja",new { CheckINId=model.CheckInId, SmjestajId=model.Smjestaj.Id });
        }
        public IActionResult IndexOdabranogSmjestaja(int CheckINId,int SmjestajId,string poruka,bool? greska)
        {
        
            CheckIN c = db.CheckIN.Include(x=>x.Gost).Where(x => x.Id == CheckINId).FirstOrDefault();
            Smjestaj s = db.Smjestaj.Include(x=>x.VrstaSmjestaja).Where(x => x.Id == SmjestajId).FirstOrDefault();

            RezervisanSmjestajIndexOdabranogSmjestajaVM model = new RezervisanSmjestajIndexOdabranogSmjestajaVM();

            if(greska==true)
            ViewBag.Poruka = poruka;

            model.DatumDolaska = c.DatumDolaska.ToShortDateString();
            model.DatumOdlaska = c.DatumOdlaska.ToShortDateString();
            model.GostNosioc=c.Gost.Ime+" "+c.Gost.Prezime;
            model.PodaciOSmjestaju = s.BrojSmjestaja + " " + s.VrstaSmjestaja.Naziv;
            model.SmjestajId = s.Id;
            model.CheckINId = c.Id;

            return View(model);
        }

        public IActionResult UcitajGosteNaSmjestaju( int SmjestajId)
        {
            RezervisanSmjestajUcitajGosteNaSmjestajuVM model = new RezervisanSmjestajUcitajGosteNaSmjestajuVM();

            model.Gosti = db.RezervisanSmjestaj.Include(x=>x.Gost).Where(x => x.SmjestajId == SmjestajId).Select(x => new SelectListItem {
                Text = x.Gost.Ime + "" + x.Gost.Prezime,
                Value = x.GostId.ToString()

            }).ToList();


            return PartialView(model);
        }


    }
}