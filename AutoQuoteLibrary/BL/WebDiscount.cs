using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;

namespace AutoQuoteLibrary.BL
{
    public enum Discounts
    {
        //Policy Discounts
        GROUPDISCOUNT, //Partnership
        MULTPOLICYDISCOUNT,
        PAPERLESSDISCOUNT,
        PREFERPAYERDISCOUNT,
        MINGLEMATEDISCOUNT,
        MARRIEDDISCOUNT,
        SAFESOUNDDISCOUNT,
        RETROLOYALTYDISCOUNT,
        //ysang 6715 8/27/2010
        CONTINUOUSDISCOUNT,
        //Driver Discounts
        DDCDISCOUNT,
        MATUREDRIVERDISCOUNT,
        //private static string SENIORDISCOUNT = "SENIORDISCOUNT";
        //fcaglar SSR7102 03-04-2011 - CA new quote flow
        GOODDRIVERDISCOUNT,
        HOMEOWNERSHIPDISCOUNT,

        //Vehicle Discounts
        AIRBAGDISCOUNT,
        ADISABLEDVCDISCOUNT, //Anti-Theft
        MULTICARDISCOUNT,
        PASSIVERESTRAINTDISCOUNT,
        VEHICLERECOVERYDISCOUNT
            //wsun 7409 11/10/2011 returnandsave discount 
       , COMEBACKANDSAVEDISCOUNT,
        WELCOMEBACKDISCOUNT,
        //dmetz 04-19-2012 SSR6873 - LA using GoodStudent to store Active Military discount
        GOODSTUDENTDISCOUNT
            //jrenz ssr09398 1/2/2014 kdquoteflow GA
       ,
        FOCUSEDDRIVERDISCOUNT
            , ESIGNATUREDISCOUNT
    }

    /// <summary>
    /// Summary description for WebDiscount
    /// </summary>
    public class WebDiscount
    {
        private ArrayList _discountList;

        private Hashtable _polDiscountList2;
        private Hashtable _drvDiscountList2;
        private Hashtable _vehDiscountList2;

        #region property
        public ArrayList DiscountList
        {
            get
            {
                return _discountList;
            }
        }
        public Hashtable PolDiscountList
        {
            get
            {
                return _polDiscountList2;
            }
        }
        public Hashtable DriverDiscountList
        {
            get
            {
                return _drvDiscountList2;
            }
        }
        public Hashtable VehicleDiscountList
        {
            get
            {
                return _vehDiscountList2;
            }
        }
        #endregion

        #region Initialize
        public WebDiscount()
        {
            Initialize();
        }

        private void Initialize()
        {
            _discountList = new ArrayList();

            _polDiscountList2 = new Hashtable();
            _drvDiscountList2 = new Hashtable();
            _vehDiscountList2 = new Hashtable();

        }
        #endregion
        #region LoadWebDiscountFromQuote
        public void LoadWebDiscountFromQuote(AutoQuote.Autoquote quote)
        {

            //got policy discount        
            if (quote.getPolicyInfo().getAffinityDis() > 0
                || quote.getPolicyInfo().getAlumniDis() > 0
                || quote.getPolicyInfo().getAssocDis() > 0
                || quote.getPolicyInfo().getErmfFactor() != 1)
            {
                if (!_polDiscountList2.Contains(Discounts.GROUPDISCOUNT.ToString()))
                {
                    if (quote.getCoverages().item(0).getSixMonthPremiums().getSmTGroupdiscPrem() > 0 || quote.getCoverages().item(0).getSixMonthPremiums().getSmTAffinityPrem() > 0)
                    {
                        if (quote.getCoverages().item(0).getSixMonthPremiums().getSmTGroupdiscPrem() > quote.getCoverages().item(0).getSixMonthPremiums().getSmTAffinityPrem())
                        {
                            _polDiscountList2.Add(Discounts.GROUPDISCOUNT.ToString(), quote.getCoverages().item(0).getSixMonthPremiums().getSmTGroupdiscPrem());
                        }
                        else
                        {
                            _polDiscountList2.Add(Discounts.GROUPDISCOUNT.ToString(), quote.getCoverages().item(0).getSixMonthPremiums().getSmTAffinityPrem());
                        }
                    }
                }
            }

            // wsun 7409 11/11/2011 returnandsave discount ,must after state is set
            if (quote.getPolicyInfo().getWelcomeBackDis() > 0)
            {
                if (!_polDiscountList2.Contains(Discounts.WELCOMEBACKDISCOUNT.ToString()))
                {
                    if (quote.getCoverages().item(0).getSixMonthPremiums().getSmTWelcomeBackPrem() > 0)
                    {
                        _polDiscountList2.Add(Discounts.WELCOMEBACKDISCOUNT.ToString(),
                            quote.getCoverages().item(0).getSixMonthPremiums().getSmTWelcomeBackPrem());
                    }
                }
            }
            if (quote.getPolicyInfo().getComeBackDis() > 0)
            {
                if (!_polDiscountList2.Contains(Discounts.COMEBACKANDSAVEDISCOUNT.ToString()))
                {
                    if (quote.getCoverages().item(0).getSixMonthPremiums().getSmTComeBackPrem() > 0)
                    {
                        _polDiscountList2.Add(Discounts.COMEBACKANDSAVEDISCOUNT.ToString(),
                            quote.getCoverages().item(0).getSixMonthPremiums().getSmTComeBackPrem());
                    }
                }
            }
            //staylor  02/04/2010  BA Req. 7.1.5.5  MPD and Paperless will always be shown
            //if (quote.getPolicyInfo().getMultiPolicyTest() > 0 && quote.getPolicyInfo().getMultiPolicyTest() <5)
            //{
            if (!_polDiscountList2.Contains(Discounts.MULTPOLICYDISCOUNT.ToString()))
            {
                //fcaglar SSR7102 03-07-2011 - CA new quote flow
                if (quote.getCoverages().item(0).getSixMonthPremiums().getSmTMultiPolicyPrem() > 0 ||
                    quote.getCustomer().getAddressStateCode() == "CA")
                {
                    _polDiscountList2.Add(Discounts.MULTPOLICYDISCOUNT.ToString(), quote.getCoverages().item(0).getSixMonthPremiums().getSmTMultiPolicyPrem());
                }
            }
            //}

            //PRD18433 udiaes 10/26/2011 
            if (quote.getPolicyInfo().getPaperlessDis() == 1)
            {
                if (!_polDiscountList2.Contains(Discounts.PAPERLESSDISCOUNT.ToString()))
                {
                    if (quote.getCoverages().item(0).getSixMonthPremiums().getSmTPaperlessPrem() > 0)
                    {
                        _polDiscountList2.Add(Discounts.PAPERLESSDISCOUNT.ToString(), quote.getCoverages().item(0).getSixMonthPremiums().getSmTPaperlessPrem());
                    }
                }
            }

            if (quote.getPolicyInfo().getPrefPayLevel() > 0)
            {
                //PRD11815 5/3/2010 udiaes
                if (!_polDiscountList2.Contains(Discounts.PREFERPAYERDISCOUNT.ToString()))
                {
                    switch (quote.getCustomer().getSpecialCorresNo())
                    {
                        case 1:
                            _polDiscountList2.Add(Discounts.PREFERPAYERDISCOUNT.ToString(), quote.getCoverages().item(0).getSixMonthPremiums().getSmTPref1payPrem().ToString());
                            break;
                        case 2:
                            _polDiscountList2.Add(Discounts.PREFERPAYERDISCOUNT.ToString(), quote.getCoverages().item(0).getSixMonthPremiums().getSmTPref2payPrem().ToString());
                            break;
                        case 3:
                            _polDiscountList2.Add(Discounts.PREFERPAYERDISCOUNT.ToString(), quote.getCoverages().item(0).getSixMonthPremiums().getSmTPrefPayrollPrem().ToString());
                            break;
                        case 4:
                            _polDiscountList2.Add(Discounts.PREFERPAYERDISCOUNT.ToString(), quote.getCoverages().item(0).getSixMonthPremiums().getSmTPref4payPrem().ToString());
                            break;
                        case 5:
                            _polDiscountList2.Add(Discounts.PREFERPAYERDISCOUNT.ToString(), quote.getCoverages().item(0).getSixMonthPremiums().getSmTPrefMonthlyAPrem().ToString());
                            break;
                        case 6:
                            _polDiscountList2.Add(Discounts.PREFERPAYERDISCOUNT.ToString(), quote.getCoverages().item(0).getSixMonthPremiums().getSmTPrefMonthlyBPrem().ToString());
                            break;
                        case 7:
                            _polDiscountList2.Add(Discounts.PREFERPAYERDISCOUNT.ToString(), quote.getCoverages().item(0).getSixMonthPremiums().getSmTPrefMonthlyCPrem().ToString());
                            break;
                        default:
                            _polDiscountList2.Add(Discounts.PREFERPAYERDISCOUNT.ToString(), quote.getCoverages().item(0).getSixMonthPremiums().getSmTPref1payPrem().ToString());
                            break;
                    }
                }
            }


            //tc #6823 09-20-2010 - MingleMate Discount should always be shown
            //dmetz 12-07-2011 SSR7537 - Network Discount available for all brands
            //if (quote.getPolicyInfo().getMarketBrand() == 2 && quote.getCoverages().item(0).getSixMonthPremiums().item(0).getSmMingleMatePrem() > 0)
            //udinzs PRD19867
            //udinzs SSR8414, 8102: tip portals
            //if (isPortal == true)
            //{
            //    //udinzs ssr8575
            //    string iminglediscount = addInfo.GetAddInfoValue("iminglediscount");
            //    if (iminglediscount == "true" || quote.getPolicyInfo().getMingleMateDis() == 1) //this will cover email and rate/recal
            //    {
            //        if (quote.getCoverages().item(0).getSixMonthPremiums().item(0).getSmMingleMatePrem() > 0)
            //        {
            //            if (!_polDiscountList2.Contains(Discounts.MINGLEMATEDISCOUNT.ToString()))
            //            {
            //                if (quote.getCoverages().item(0).getSixMonthPremiums().getSmTMingleMatePrem() > 0)
            //                {
            //                    _polDiscountList2.Add(Discounts.MINGLEMATEDISCOUNT.ToString(), quote.getCoverages().item(0).getSixMonthPremiums().getSmTMingleMatePrem());
            //                }
            //            }
            //        }
            //    }
            //}
            //else
            //{
                if (quote.getCoverages().item(0).getSixMonthPremiums().item(0).getSmMingleMatePrem() > 0)
                {
                    if (!_polDiscountList2.Contains(Discounts.MINGLEMATEDISCOUNT.ToString()))
                    {
                        if (quote.getCoverages().item(0).getSixMonthPremiums().getSmTMingleMatePrem() > 0)
                        {
                            _polDiscountList2.Add(Discounts.MINGLEMATEDISCOUNT.ToString(), quote.getCoverages().item(0).getSixMonthPremiums().getSmTMingleMatePrem());
                        }
                    }
                }
            //}

            if (quote.getPolicyInfo().getMarriedDis() > 0)
            {
                if (!_polDiscountList2.Contains(Discounts.MARRIEDDISCOUNT.ToString()))
                {
                    if (quote.getCoverages().item(0).getSixMonthPremiums().getSmTMarriedPrem() > 0)
                    {
                        _polDiscountList2.Add(Discounts.MARRIEDDISCOUNT.ToString(), quote.getCoverages().item(0).getSixMonthPremiums().getSmTMarriedPrem());
                    }
                }
            }

            if (quote.getPolicyInfo().getSafeSoundDis() > 0)
            {
                if (!_polDiscountList2.Contains(Discounts.SAFESOUNDDISCOUNT.ToString()))
                {
                    if (quote.getCoverages().item(0).getSixMonthPremiums().getSmTSafeSoundPrem() > 0)
                    {
                        _polDiscountList2.Add(Discounts.SAFESOUNDDISCOUNT.ToString(), quote.getCoverages().item(0).getSixMonthPremiums().getSmTSafeSoundPrem());
                    }
                }
            }

            //dmetz 04-23-2012 SSR6873 - LA Active Military discount using Good Student Discount field.
            if (quote.getCoverages().item(0).getSixMonthPremiums().getSmTGoodStudentPrem() > 0)
            {
                if (!_polDiscountList2.Contains(Discounts.GOODSTUDENTDISCOUNT.ToString()))
                {
                    _polDiscountList2.Add(Discounts.GOODSTUDENTDISCOUNT.ToString(), quote.getCoverages().item(0).getSixMonthPremiums().getSmTGoodStudentPrem());
                }
            }

            if (quote.getCoverages().item(0).getSixMonthPremiums().count() > 0 && quote.getPolicyInfo().getRetroLoyaltyLevel() > 0 && quote.getCoverages().item(0).getSixMonthPremiums().item(0).getSmRetroLoyaltyPrem() > 0)
            {
                if (!_polDiscountList2.Contains(Discounts.RETROLOYALTYDISCOUNT.ToString()))
                {
                    if (quote.getCoverages().item(0).getSixMonthPremiums().getSmTRetroLoyaltyPrem() > 0)
                    {
                        _polDiscountList2.Add(Discounts.RETROLOYALTYDISCOUNT.ToString(), quote.getCoverages().item(0).getSixMonthPremiums().getSmTRetroLoyaltyPrem());
                    }
                }
            }

            //ysang 6715 8/27/2010 
            if (quote.getPolicyInfo().getNoOfDaysLapsed() > 1)
            {
                string state = quote.getCustomer().getAddressStateCode();
                if ("IA~SC~NJ~WI~VA".IndexOf(state) != -1)
                {
                    //quote.getCoverages().item(0).getSixMonthPremiums().item(0).getSmContCoveragePrem();
                    if (!_polDiscountList2.Contains(Discounts.CONTINUOUSDISCOUNT.ToString())) //&& quote.getCoverages().item(0).getSixMonthPremiums().item(0).getsmt)
                    {
                        AutoQuote.SixMonthPremiums sixMonthPrems = quote.getCoverages().item(0).getSixMonthPremiums();
                        double conCov = 0;// sixMonthPrems.getSmTContCoveragePrem();
                        conCov = sixMonthPrems.getSmTContCoveragePrem();// == 0 ? 12 : conCov;
                        _polDiscountList2.Add(Discounts.CONTINUOUSDISCOUNT.ToString(), conCov.ToString());

                    }
                }

            }
            //udinzs ssr6845 vibe 
            //dmetz 06-15-2011 SSR7246 - NJ, TX
            //dmetz 08-25-2011 SSR7965 - MI
            //dmetz 09-02-2011 SSR6871 - CT, KS, TN
            //if (quote.getCustomer().getAddressStateCode() == "AZ" || quote.getCustomer().getAddressStateCode() == "OH" || quote.getCustomer().getAddressStateCode() == "NJ" || quote.getCustomer().getAddressStateCode() == "TX")
            string thisState = quote.getCustomer().getAddressStateCode();
            //SSR08080 udiaes state test is unnecessary.  it can be pulled from the xml
            //            if ("AZ~OH~NJ~TX~MI~CT~KS~TN".IndexOf(thisState) != -1)
            //            {
            if (quote.getCustomer().getRentOwnTest() == 1)
            {
                if (!_polDiscountList2.Contains(Discounts.HOMEOWNERSHIPDISCOUNT.ToString()))
                {
                    AutoQuote.SixMonthPremiums sixMonthPrems = quote.getCoverages().item(0).getSixMonthPremiums();
                    double homeOwnerPrem = 0;
                    homeOwnerPrem = sixMonthPrems.getSmTHomeownerPrem();
                    _polDiscountList2.Add(Discounts.HOMEOWNERSHIPDISCOUNT.ToString(), homeOwnerPrem.ToString());
                }
            }
            //            }
            //driver discounts
            for (int i = 0; i < quote.getDrivers().count(); i++)
            {
                DisplayDriver displayDriver = new DisplayDriver();
                AutoQuote.Driver drv = quote.getDrivers().item(i);

                ArrayList dlist = new ArrayList();
                ArrayList drivName = new ArrayList();
                string sName = "";
                if (drv.getDrivMiddle().Length == 0)
                {
                    sName = string.Format("{0} {1}", drv.getDrivFirst(), drv.getDrivLast());

                }
                else
                {
                    sName = string.Format("{0} {1} {2}", drv.getDrivFirst(), drv.getDrivMiddle(), drv.getDrivLast());

                }
                //for email quote
                displayDriver.FirstName = drv.getDrivFirst();
                displayDriver.LastName = drv.getDrivLast();
                displayDriver.MiddleInit = drv.getDrivMiddle();
                displayDriver.BirthDate = drv.getBirthDateOfDriv().ToString("g");
                displayDriver.Marital = drv.getDrivMarriedSingle();


                //move to veh policy, now move to policy level

                if (drv.getDdcDiscount() > 0 && drv.getDdcDiscount() < 3)
                {
                    //ysang 7246 7/87/2011
                    if (quote.getCustomer().getAddressStateCode() == "PA" || quote.getCustomer().getAddressStateCode() == "NJ")
                    {
                        if (!_polDiscountList2.Contains(Discounts.MATUREDRIVERDISCOUNT.ToString()))
                        {
                            //if (quote.getCoverages().item(0).getSixMonthPremiums().item(0).getSmDdcPrem() > 0)
                            //{
                            //    _polDiscountList2.Add(Discounts.MATUREDRIVERDISCOUNT.ToString(), quote.getCoverages().item(0).getSixMonthPremiums().item(0).getSmDdcPrem());
                            //}
                            //ysang prd17792 8/4/2011 should get total ddc premium quote.getCoverages().item(0).getSixMonthPremiums().getSmTDdcPrem()
                            if (quote.getCoverages().item(0).getSixMonthPremiums().getSmTDdcPrem() > 0)
                            {
                                _polDiscountList2.Add(Discounts.MATUREDRIVERDISCOUNT.ToString(), quote.getCoverages().item(0).getSixMonthPremiums().getSmTDdcPrem());
                            }
                        }
                    }
                    else
                    {
                        if (!dlist.Contains(Discounts.DDCDISCOUNT.ToString()))
                        {
                            //tc #7433 01-13-2010 - Defensive Driver Discount does not always have a premium and is on the driver level
                            //if (quote.getCoverages().item(0).getSixMonthPremiums().item(0).getSmDdcPrem() > 0)
                            //{
                            dlist.Add(Discounts.DDCDISCOUNT.ToString());
                            //_drvDiscountList2.Add(Discounts.DDCDISCOUNT.ToString(), quote.getCoverages().item(0).getSixMonthPremiums().item(0).getSmDdcPrem());
                            //udinzs PRD20749 check is ddc is already applied to driver
                            if (!_drvDiscountList2.Contains(Discounts.DDCDISCOUNT.ToString()))
                            {
                                _drvDiscountList2.Add(Discounts.DDCDISCOUNT.ToString(), quote.getCoverages().item(0).getSixMonthPremiums().getSmTDdcPrem());
                            }
                            //}
                        }
                    }
                }

                //move to policy level
                //jrenz #7492 VA Vibe 3/3/2011
                if ((drv.getMatureDriverDis() > 0 && drv.getMatureDriverDis() < 3) ||
                    (quote.getCoverages().item(0).getSixMonthPremiums().getSmTMatureDrivPrem() > 0))
                {
                    if (!_drvDiscountList2.Contains(Discounts.MATUREDRIVERDISCOUNT.ToString()))
                    {
                        //jrenz #7492 VA Vibe 2/18/2011 - new property for policy level mature discount.
                        _drvDiscountList2.Add(Discounts.MATUREDRIVERDISCOUNT.ToString(), quote.getCoverages().item(0).getSixMonthPremiums().getSmTMatureDrivPrem());
                        //_drvDiscountList2.Add(Discounts.MATUREDRIVERDISCOUNT.ToString(), quote.getCoverages().item(0).getSixMonthPremiums().item(0).getSmMatureDrivPrem());
                        //}
                    }
                }

                //fcaglar SSR7102 03-07-2011 - CA new quote flow
                if (quote.getCustomer().getAddressStateCode() == "CA")
                {
                    if (drv.getGoodDriverDis() > 0)
                    {
                        if (!_drvDiscountList2.Contains(Discounts.GOODDRIVERDISCOUNT.ToString()))
                        {
                            _drvDiscountList2.Add(Discounts.GOODDRIVERDISCOUNT.ToString(), quote.getCoverages().item(0).getSixMonthPremiums().item(0).getSmGoodDriverPrem());
                        }
                    }
                }

                //add into ht                
                displayDriver.FullName = sName;
                displayDriver.DiscountNames = dlist;
                //if (!_drvDiscountList.Contains(i))
                //    _drvDiscountList.Add(i, displayDriver);
            }

            //vehicle discounts
            for (int i = 0; i < quote.getVehicles().count(); i++)
            {

                DisplayVehicle displayVehicle = new DisplayVehicle();
                AutoQuote.Vehicle veh = quote.getVehicles().item(i);

                ArrayList vlist = new ArrayList();
                string sVehicle = string.Format(" {0} {1} {2}", veh.getVehYear(), veh.getVehMake(), veh.getVehModel());

                //for policy level
                if (veh.getMultiCarDis() > 0 && veh.getMultiCarDis() < 3)
                {
                    if (!_polDiscountList2.Contains(Discounts.MULTICARDISCOUNT.ToString()))
                    {
                        if (quote.getCoverages().item(0).getSixMonthPremiums().getSmTMultiCarPrem() > 0)
                        {
                            _polDiscountList2.Add(Discounts.MULTICARDISCOUNT.ToString(), quote.getCoverages().item(0).getSixMonthPremiums().getSmTMultiCarPrem());
                        }
                    }
                }

                //veh policy
                if (veh.getAirBagTest() > 0 && veh.getAirBagTest() < 3)
                {
                    if (quote.getCoverages().item(0).getSixMonthPremiums().getSmTAirBagPrem() > 0)
                    {
                        if (!vlist.Contains(Discounts.AIRBAGDISCOUNT.ToString()))
                            vlist.Add(Discounts.AIRBAGDISCOUNT.ToString());
                        if (!_vehDiscountList2.Contains(Discounts.AIRBAGDISCOUNT.ToString()))
                            _vehDiscountList2.Add(Discounts.AIRBAGDISCOUNT.ToString(), quote.getCoverages().item(0).getSixMonthPremiums().getSmTAirBagPrem());
                    }
                }

                //for Anti-theft Device 
                //dmetz 08-02-2011 SSR7246/TST10880 - NJ uses WindowEtchingTest && DisablingDevice can be 0-5
                if (veh.getDisablingDevice() > 0 || veh.getWindowEtchingTest() > 0)
                {
                    if ((quote.getCustomer().getAddressStateCode() == "MI" && veh.getDisablingDevice() < 9)
                        || (quote.getCustomer().getAddressStateCode() == "NJ" && (veh.getWindowEtchingTest() > 0 || veh.getDisablingDevice() < 6))
                        || veh.getDisablingDevice() < 5)
                    {
                        if (quote.getCoverages().item(0).getSixMonthPremiums().getSmTDisablingDevicePrem() > 0)
                        {
                            if (!vlist.Contains(Discounts.ADISABLEDVCDISCOUNT.ToString()))
                                vlist.Add(Discounts.ADISABLEDVCDISCOUNT.ToString());
                            if (!_vehDiscountList2.Contains(Discounts.ADISABLEDVCDISCOUNT.ToString()))
                                _vehDiscountList2.Add(Discounts.ADISABLEDVCDISCOUNT.ToString(), quote.getCoverages().item(0).getSixMonthPremiums().getSmTDisablingDevicePrem());
                        }
                    }

                    //fcaglar SSR7102 03-04-2011 - CA new quote flow
                    if (quote.getCustomer().getAddressStateCode() == "CA")
                    {
                        if (quote.getCoverages().item(0).getSixMonthPremiums().getSmTDisablingDevicePrem() > 0)
                        {
                            if (!vlist.Contains(Discounts.VEHICLERECOVERYDISCOUNT.ToString()))
                                vlist.Add(Discounts.VEHICLERECOVERYDISCOUNT.ToString());

                            if (!_vehDiscountList2.Contains(Discounts.VEHICLERECOVERYDISCOUNT.ToString()))
                                _vehDiscountList2.Add(Discounts.VEHICLERECOVERYDISCOUNT.ToString(), quote.getCoverages().item(0).getSixMonthPremiums().getSmTDisablingDevicePrem());
                        }
                    }
                }
                else
                {
                    //shares the same Premium field as Disabled Device Discount
                    if (veh.getVehicleRecoveryTest() > 0 && veh.getVehicleRecoveryTest() < 3)
                    {
                        if (quote.getCoverages().item(0).getSixMonthPremiums().getSmTDisablingDevicePrem() > 0)
                        {
                            if (!vlist.Contains(Discounts.VEHICLERECOVERYDISCOUNT.ToString()))
                                vlist.Add(Discounts.VEHICLERECOVERYDISCOUNT.ToString());
                            if (!_vehDiscountList2.Contains(Discounts.VEHICLERECOVERYDISCOUNT.ToString()))
                                _vehDiscountList2.Add(Discounts.VEHICLERECOVERYDISCOUNT.ToString(), quote.getCoverages().item(0).getSixMonthPremiums().getSmTDisablingDevicePrem());
                        }
                    }
                }

                //jrenz #PRD12933 7/19/2010 do not check PassiveRestraint indicator,
                // just check for dollar amount greater than 0.
                //if (veh.getPassiveRestraint() == 2)
                //{
                if (quote.getCoverages().item(0).getSixMonthPremiums().getSmTPassiveRestraintPrem() > 0)
                {
                    if (!vlist.Contains(Discounts.PASSIVERESTRAINTDISCOUNT.ToString()))
                        vlist.Add(Discounts.PASSIVERESTRAINTDISCOUNT.ToString());
                    if (!_vehDiscountList2.Contains(Discounts.PASSIVERESTRAINTDISCOUNT.ToString()))
                        _vehDiscountList2.Add(Discounts.PASSIVERESTRAINTDISCOUNT.ToString(), quote.getCoverages().item(0).getSixMonthPremiums().getSmTPassiveRestraintPrem());
                }
                //}

                //if (veh.getSafeVehDis() > 0 && veh.getSafeVehDis() < 3)
                //{
                //    if (!vlist.Contains(SAFEVEHDISCOUNT))
                //        vlist.Add(SAFEVEHDISCOUNT);
                //}

                //if ((int)veh.getRetiredDrivDis() > 0 && (int)veh.getRetiredDrivDis() < 3)
                //{
                //    if (!vlist.Contains(SENIORDISCOUNT))
                //        vlist.Add(SENIORDISCOUNT);
                //}

                //JRENZ ssr09398 KDQUOTEFLOW GA 1/2/2014
                if ((quote.getCustomer().getAddressStateCode() == "GA") &&
                    (veh.getWebData() == 1))
                {
                    if (!vlist.Contains(Discounts.FOCUSEDDRIVERDISCOUNT.ToString()))
                        vlist.Add(Discounts.FOCUSEDDRIVERDISCOUNT.ToString());
                    if (!_vehDiscountList2.Contains(Discounts.FOCUSEDDRIVERDISCOUNT.ToString()))
                        _vehDiscountList2.Add(Discounts.FOCUSEDDRIVERDISCOUNT.ToString(), "1");

                }
                if (("KY,WA".Contains(quote.getCustomer().getAddressStateCode())) &&
                                    (veh.getWebData() == 1))
                {
                    if (!vlist.Contains(Discounts.ESIGNATUREDISCOUNT.ToString()))
                        vlist.Add(Discounts.ESIGNATUREDISCOUNT.ToString());
                    if (!_vehDiscountList2.Contains(Discounts.ESIGNATUREDISCOUNT.ToString()))
                        _vehDiscountList2.Add(Discounts.ESIGNATUREDISCOUNT.ToString(), "1");

                }


                displayVehicle.FullName = sVehicle;
                displayVehicle.DiscountNames = vlist;
                //_vehDiscountList.Add(i, displayVehicle);
            }

        }
        #endregion
    }

    #region class Driver
    public class DisplayDriver
    {
        private string _drvLastName = "";
        private string _drvfstName = "";
        private string _drvMiddleInit = "";
        private string _drvBirthDate = "";
        private int _drvMarital;
        private ArrayList _discountNames = new ArrayList();
        private string _fullName = "";

        public string FullName
        {
            get { return _fullName; }
            set { _fullName = value; }
        }
        public string LastName
        {
            get { return _drvLastName; }
            set { _drvLastName = value; }
        }
        public string FirstName
        {
            get { return _drvfstName; }
            set { _drvfstName = value; }
        }
        public string MiddleInit
        {
            get { return _drvMiddleInit; }
            set { _drvMiddleInit = value; }
        }
        public string BirthDate
        {
            get { return _drvBirthDate; }
            set { _drvBirthDate = value; }
        }
        public int Marital
        {
            get { return _drvMarital; }
            set { _drvMarital = value; }

        }
        public ArrayList DiscountNames
        {
            get { return _discountNames; }
            set { _discountNames = value; }
        }
    }
    #endregion

    #region class DisplayVehicle
    public class DisplayVehicle
    {
        private ArrayList _discountNames = new ArrayList();
        private string _fullName = "";
        public ArrayList DiscountNames
        {
            get { return _discountNames; }
            set { _discountNames = value; }
        }
        public string FullName
        {
            get { return _fullName; }
            set { _fullName = value; }
        }
    }
    #endregion
}
