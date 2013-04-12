using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using PayPal;
using PayPal.Version940;

namespace PayPalConsoleApp
{
    public static class Program
    {
        [STAThread]
        public static void Main(string[] args)
        {
            NameValueCollection nvc0 = new NameValueCollection();

            nvc0["xpto"] = "abc1";
            nvc0.Add("xpto", "abc2");
            nvc0.Add("xpto", "");

            nvc0.Add("cde", null);
            var cde = nvc0["cde"];
            var cde2 = nvc0.GetValues("cde");

            var xpto = nvc0["xpto"];
            var xpto2 = nvc0.GetValues("xpto");


            Console.SetBufferSize(170, 500);
            Console.SetWindowSize(170, 80);

            DigitalGoodsTest();

            Console.ReadKey();

            Console.Clear();

            PayPalSetExpressCheckoutOperation ecop0 = BuildExpressCheckoutOperation1();
            var nvc1 = ecop0.ToNameValueCollection();

            foreach (var key in nvc1.AllKeys)
            {
                var value = nvc1[key];

                Console.ForegroundColor = ConsoleColor.Gray;
                Console.Write(key);
                Console.ForegroundColor = ConsoleColor.Blue;
                Console.Write(" = ");
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Write(@"""");
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.Write(value);
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Write(@"""");
                Console.WriteLine();
            }

            Console.SetWindowPosition(0, 0);

            Console.ReadKey();

            Console.Clear();

            var ecop2 = nvc1.LoadToPayPalModelType<PayPalSetExpressCheckoutOperation>();

            var nvc2 = ecop2.ToNameValueCollection();

            foreach (var key in nvc2.AllKeys)
            {
                var value = nvc2[key];

                Console.ForegroundColor = ConsoleColor.Gray;
                Console.Write(key);
                Console.ForegroundColor = ConsoleColor.Blue;
                Console.Write(" = ");
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Write(@"""");
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.Write(value);
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Write(@"""");
                Console.WriteLine();
            }

            Console.ReadKey();

            var value2 = EnumHelper.GetAttributeOfType<StringValueAttribute, TestEnum>("Text1");
        }

        /// <summary>
        /// Tests whether it is possible or not to have multiple payments when there are digital goods.
        /// </summary>
        /// <remarks>
        /// This test will test all possibilities of
        /// setting new payments/items collections,
        /// setting/adding a specific payments/items collection items,
        /// and setting values,
        /// in any order of execution.
        /// </remarks>
        private static void DigitalGoodsTest()
        {
            string msg = "{0} - {1}";

            Action<int[], int, int> test = (int[] arrange5in5, int paymentChooser, int itemChooser) =>
            {
                // Creating objects.
                PayPalSetExpressCheckoutOperation ecop = new PayPalSetExpressCheckoutOperation
                {
                    PaymentRequests = new PayPalList<PayPalPaymentRequest>
                    {
                        new PayPalPaymentRequest { Items = new PayPalList<PayPalPaymentRequestItem> { new PayPalPaymentRequestItem(), new PayPalPaymentRequestItem() } },
                        new PayPalPaymentRequest { Items = new PayPalList<PayPalPaymentRequestItem> { new PayPalPaymentRequestItem(), new PayPalPaymentRequestItem() } },
                    }
                };

                PayPalList<PayPalPaymentRequest> listPayments = new PayPalList<PayPalPaymentRequest> {
                    new PayPalPaymentRequest { Items = new PayPalList<PayPalPaymentRequestItem> { new PayPalPaymentRequestItem(), new PayPalPaymentRequestItem() } },
                    new PayPalPaymentRequest { Items = new PayPalList<PayPalPaymentRequestItem> { new PayPalPaymentRequestItem(), new PayPalPaymentRequestItem() } }
                };

                PayPalPaymentRequest payment = new PayPalPaymentRequest { Items = new PayPalList<PayPalPaymentRequestItem> { new PayPalPaymentRequestItem(), new PayPalPaymentRequestItem() } };

                PayPalList<PayPalPaymentRequestItem> listItems = new PayPalList<PayPalPaymentRequestItem> { new PayPalPaymentRequestItem(), new PayPalPaymentRequestItem() };

                PayPalPaymentRequestItem item = new PayPalPaymentRequestItem();

                // Creating delegates.
                Action listPaymentsSet = () => ecop.PaymentRequests = listPayments;

                Action paymentSet = () => listPayments[0] = payment;
                Action paymentAdd = () => listPayments.Add(payment);

                Action listItemsSet = () => payment.Items = listItems;

                Action itemAdd = () => listItems.Add(item);
                Action itemSet = () => listItems[0] = item;

                Action digitalSet = () => item.Category = ItemCategory.Digital;

                // Selecting delegates.
                Action paymentAction = (new[] { paymentSet, paymentAdd })[paymentChooser];
                Action itemAction = (new[] { itemSet, itemAdd })[itemChooser];

                Action[] all = (new[] { listPaymentsSet, paymentAction, listItemsSet, itemAction, digitalSet });

                Action[] sequence = (new[] { all[arrange5in5[0]], all[arrange5in5[1]], all[arrange5in5[2]], all[arrange5in5[3]], all[arrange5in5[4]] });

                // Executing test.
                bool ok = true;
                Exception ex = null;
                try
                {
                    foreach (var action in sequence)
                        action();

                    ok = false;
                }
                catch (Exception ex1)
                {
                    ex = ex1;
                }
                finally
                {
                    Console.WriteLine(msg, ok ? "SUCCESS" : "FAILED", ex != null ? ex.Message : null);
                }
            };

            // arrangements of 5 elements in 5 slots.
            int index = 0;
            var arranges5in5 = new int[5 * 4 * 3 * 2 * 1][];
            for (int itA = 0; itA < 5; itA++)
                for (int itB = 0; itB < 4; itB++)
                    for (int itC = 0; itC < 3; itC++)
                        for (int itD = 0; itD < 2; itD++)
                            for (int itE = 0; itE < 1; itE++)
                            {
                                List<int> list = new List<int>(new[] { 0, 1, 2, 3, 4 });
                                arranges5in5[index++] = new[] {
                                    list.Slice(itA),
                                    list.Slice(itB),
                                    list.Slice(itC),
                                    list.Slice(itD),
                                    list.Slice(itE),
                                };
                            }

            // Trying every variation: (5*4*3*2) * 2 * 2 = 480 alternatives.
            foreach (var arrg in arranges5in5)
                for (int it1 = 0; it1 < 2; it1++)
                    for (int it2 = 0; it2 < 2; it2++)
                        test(arrg, it1, it2);
        }

        private static PayPalSetExpressCheckoutOperation BuildExpressCheckoutOperation1()
        {
            PayPalSetExpressCheckoutOperation ecop = new PayPalSetExpressCheckoutOperation
            {
                Credential = new PayPalSignatureCredential
                {
                    ApiPassword = "ApiPassword",
                    ApiSignature = "ApiSignature",
                    ApiUserName = "ApiUserName",
                },

                ReturnURL = "http://mydomain/returnUrl",
                CancelURL = "http://mydomain/cancelUrl",

                LocaleCode = LocaleCode.Undefined,

                SurveyChoice = new List<string>
                {
                    "Item 1",
                    "Item 2",
                    "Item 3",
                },
                PaymentRequests = new PayPalList<PayPalPaymentRequest>
                {
                    new PayPalPaymentRequest
                    {
                        Action = PaymentActionCode.Sale,
                        ItemAmount = 180.00m,
                        Description = "Cerebello - Pacote premium",
                        Items = new PayPalList<PayPalPaymentRequestItem>
                        {
                            new PayPalPaymentRequestItem
                            {
                                Amount = 170.00m,
                                Description = "Cerebello - Plano premium",
                                Quantity = 1,
                                Category = ItemCategory.Digital,
                            },
                            new PayPalPaymentRequestItem
                            {
                                Amount = 50.00m,
                                Description = "Cerebello - Envio de Sms",
                                Quantity = 1,
                                Category = ItemCategory.Digital,
                            },
                            new PayPalPaymentRequestItem
                            {
                                Amount = 40.00m,
                                Description = "Cerebello - Suporte via chat",
                                Quantity = 1,
                                Category = ItemCategory.Digital,
                            },
                            new PayPalPaymentRequestItem
                            {
                                Amount = -60.00m,
                                Description = "Cupom de desconto (CEB-0A9B8C13EA9D)",
                                Quantity = 1,
                                Category = ItemCategory.Digital,
                            },
                            new PayPalPaymentRequestItem
                            {
                                Description = "Manual de instruções",
                                Quantity = 1,
                                Category = ItemCategory.Physical,
                            },
                        }
                    },
                }
            };
            return ecop;
        }
    }

    public enum TestEnum
    {
        [StringValue("VAL-1")]
        Value1 = 1,

        [StringValue("TXT-1")]
        Text1 = 1,
    }
}
