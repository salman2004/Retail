
namespace CDC
{
    namespace Commerce.Runtime.AggregateSalesLines
    {
        using System;
        using System.Collections.Generic;
        using System.Collections.ObjectModel;
        using Microsoft.Dynamics.Commerce.Runtime;
        using Microsoft.Dynamics.Commerce.Runtime.DataModel;
        using Microsoft.Dynamics.Commerce.Runtime.Extensions;
        using Microsoft.Dynamics.Commerce.Runtime.Framework;

        internal class SalesLineAggregationHelper
        {
            private class SalesLinePair
            {
                /// <summary>
                /// Gets or sets the anchor sales line used to compare with other sales lines to be aggregated.
                /// </summary>
                internal SalesLine Anchor { get; set; }

                /// <summary>
                /// Gets or sets the aggregated sales line.
                /// </summary>
                internal SalesLine AggregatedLine { get; set; }

                internal SalesLinePair(SalesLine anchor)
                {
                    ThrowIf.Null(anchor, "anchor");
                    Anchor = anchor;
                    AggregatedLine = anchor.Clone<SalesLine>();
                }

                internal void Aggregate(SalesLine salesLine)
                {
                    AggregatedLine.GrossAmount += salesLine.GrossAmount;
                    AggregatedLine.Quantity += salesLine.Quantity;
                    AggregatedLine.TaxAmount += salesLine.TaxAmount;
                    AggregatedLine.NetAmount += salesLine.NetAmount;
                    AggregatedLine.TaxAmountExemptInclusive += salesLine.TaxAmountExemptInclusive;
                    AggregatedLine.TaxAmountInclusive += salesLine.TaxAmountInclusive;
                    AggregatedLine.TaxAmountExclusive += salesLine.TaxAmountExclusive;
                    AggregatedLine.NetAmountWithAllInclusiveTax += salesLine.NetAmountWithAllInclusiveTax;
                    AggregatedLine.TotalAmount += salesLine.TotalAmount;
                    AggregatedLine.NetAmountWithoutTax += salesLine.NetAmountWithoutTax;
                    AggregatedLine.DiscountAmount += salesLine.DiscountAmount;
                    AggregatedLine.TotalDiscount += salesLine.TotalDiscount;
                    AggregatedLine.LineDiscount += salesLine.LineDiscount;
                    AggregatedLine.PeriodicDiscount += salesLine.PeriodicDiscount;
                    AggregatedLine.LineManualDiscountAmount += salesLine.LineManualDiscountAmount;
                    AggregatedLine.QuantityRemained = AggregateNullableDecimal(AggregatedLine.QuantityRemained, salesLine.QuantityRemained);
                    AggregatedLine.QuantityShipped = AggregateNullableDecimal(AggregatedLine.QuantityShipped, salesLine.QuantityShipped);
                    AggregatedLine.QuantityInvoiced = AggregateNullableDecimal(AggregatedLine.QuantityInvoiced, salesLine.QuantityInvoiced);
                    AggregatedLine.QuantityOrdered = AggregateNullableDecimal(AggregatedLine.QuantityOrdered, salesLine.QuantityOrdered);
                    AggregatedLine.QuantityCanceled = AggregateNullableDecimal(AggregatedLine.QuantityCanceled, salesLine.QuantityCanceled);
                    AggregatedLine.QuantityPicked = AggregateNullableDecimal(AggregatedLine.QuantityPicked, salesLine.QuantityPicked);
                    AggregatedLine.QuantityPacked = AggregateNullableDecimal(AggregatedLine.QuantityPacked, salesLine.QuantityPacked);
                    AggregatedLine.SavedQuantity = AggregateNullableDecimal(AggregatedLine.SavedQuantity, salesLine.SavedQuantity);
                    AggregatedLine.DeliveryModeChargeAmount = AggregateNullableDecimal(AggregatedLine.DeliveryModeChargeAmount, salesLine.DeliveryModeChargeAmount);
                    AggregatedLine.LoyaltyDiscountAmount += salesLine.LoyaltyDiscountAmount;
                    AggregatedLine.QuantityDiscounted += salesLine.QuantityDiscounted;
                    AggregatedLine.UnitQuantity += salesLine.UnitQuantity;
                    AggregatedLine.AttributeValues.AddRange(salesLine.AttributeValues);
                    AggregatedLine.ReasonCodeLines.AddRange(salesLine.ReasonCodeLines);
                    AggregatedLine.LoyaltyRewardPointLines.AddRange(salesLine.LoyaltyRewardPointLines);

                    if (!AggregatedLine.DiscountLines.IsNullOrEmpty())
                    {
                        HashSet<DiscountLine> discountLines = new HashSet<DiscountLine>(new GenericEqualityComparer<DiscountLine>((DiscountLine x, DiscountLine y) => x.OfferId == y.OfferId, (DiscountLine x) => x.OfferId.GetHashCode()));
                        discountLines.AddRange(AggregatedLine.DiscountLines);
                        discountLines.AddRange(salesLine.DiscountLines);

                        AggregatedLine.DiscountLines.Clear();
                        AggregatedLine.DiscountLines.AddRange(discountLines);
                    }
                }

                /// <summary>
                /// Returns a boolean value indicating whether or not the given sales line can be aggregated with
                /// the anchor sales line of this sales lines pair.
                /// </summary>
                /// <param name="lineToAggregate">The given sales line to determine.</param>
                /// <param name="isAggregateSalesLinesQuantity">AggregateSalesLinesQuantity configuration key.</param>
                /// <returns>True if can be aggregated, otherwise false.</returns>
                internal bool CanAggregate(SalesLine lineToAggregate, bool isAggregateSalesLinesQuantity)
                {
                    if (!isAggregateSalesLinesQuantity)
                    {
                        if (Anchor.ItemId == lineToAggregate.ItemId && Anchor.ProductId == lineToAggregate.ProductId && Anchor.TaxRatePercent == lineToAggregate.TaxRatePercent && string.Equals(Anchor.ItemTaxGroupId, lineToAggregate.ItemTaxGroupId, StringComparison.Ordinal) && string.Equals(Anchor.StaffId, lineToAggregate.StaffId, StringComparison.Ordinal) && string.Equals(Anchor.Description, lineToAggregate.Description, StringComparison.Ordinal) && string.Equals(Anchor.OriginLineId, lineToAggregate.OriginLineId, StringComparison.Ordinal) && string.Equals(Anchor.TaxOverrideCode, lineToAggregate.TaxOverrideCode, StringComparison.Ordinal) && Anchor.Price == lineToAggregate.Price && string.Equals(Anchor.Barcode, lineToAggregate.Barcode, StringComparison.Ordinal) && Anchor.EntryMethodTypeValue == lineToAggregate.EntryMethodTypeValue && Anchor.MasterProductId == lineToAggregate.MasterProductId && Anchor.ListingId == lineToAggregate.ListingId && AreShippingAddressesEqual(Anchor.ShippingAddress, lineToAggregate.ShippingAddress) && string.Equals(Anchor.DeliveryMode, lineToAggregate.DeliveryMode, StringComparison.Ordinal) && string.Equals(Anchor.Comment, lineToAggregate.Comment, StringComparison.Ordinal) && Anchor.RequestedDeliveryDate.Equals(lineToAggregate.RequestedDeliveryDate) && string.Equals(Anchor.InventoryLocationId, lineToAggregate.InventoryLocationId, StringComparison.Ordinal) && string.Equals(Anchor.WarehouseLocation, lineToAggregate.WarehouseLocation, StringComparison.Ordinal) && string.Equals(Anchor.InventoryStatusId, lineToAggregate.InventoryStatusId, StringComparison.Ordinal) && string.Equals(Anchor.LicensePlate, lineToAggregate.LicensePlate, StringComparison.Ordinal) && string.Equals(Anchor.InventoryDimensionId, lineToAggregate.InventoryDimensionId, StringComparison.Ordinal) && Anchor.ReservationId.Equals(lineToAggregate.ReservationId) && Anchor.Status == lineToAggregate.Status && Anchor.SalesStatus == lineToAggregate.SalesStatus && Anchor.ProductSource == lineToAggregate.ProductSource && Anchor.ChargeLines.IsNullOrEmpty() && lineToAggregate.ChargeLines.IsNullOrEmpty() && Anchor.FulfillmentStoreId == lineToAggregate.FulfillmentStoreId && string.Equals(Anchor.SerialNumber, lineToAggregate.SerialNumber, StringComparison.Ordinal) && string.Equals(Anchor.BatchId, lineToAggregate.BatchId, StringComparison.Ordinal) && Anchor.DeliveryModeChargeAmount == lineToAggregate.DeliveryModeChargeAmount && string.Equals(Anchor.UnitOfMeasureSymbol, lineToAggregate.UnitOfMeasureSymbol, StringComparison.Ordinal) && Anchor.CatalogId == lineToAggregate.CatalogId && string.Equals(Anchor.ElectronicDeliveryEmailAddress, lineToAggregate.ElectronicDeliveryEmailAddress, StringComparison.OrdinalIgnoreCase) && string.Equals(Anchor.ElectronicDeliveryEmailContent, lineToAggregate.ElectronicDeliveryEmailContent, StringComparison.OrdinalIgnoreCase) && string.Equals(Anchor.TradeAgreementPriceGroup, lineToAggregate.TradeAgreementPriceGroup, StringComparison.Ordinal) && Anchor.Blocked == lineToAggregate.Blocked && Anchor.Found == lineToAggregate.Found && Anchor.UnitQuantity == lineToAggregate.UnitQuantity && Anchor.LineMultilineDiscOnItem == lineToAggregate.LineMultilineDiscOnItem && Anchor.WasChanged == lineToAggregate.WasChanged && Anchor.TrackingId == lineToAggregate.TrackingId)
                        {
                            return string.Equals(Anchor.CommissionSalesGroup, lineToAggregate.CommissionSalesGroup, StringComparison.Ordinal);
                        }
                        return false;
                    }

                    return Anchor.Quantity == lineToAggregate.Quantity;
                }

                private decimal? AggregateNullableDecimal(decimal? aggregatedValue, decimal? valueToAggregate)
                {
                    if (!aggregatedValue.HasValue && !valueToAggregate.HasValue)
                    {
                        return null;
                    }

                    if (!aggregatedValue.HasValue)
                    {
                        return valueToAggregate;
                    }

                    if (!valueToAggregate.HasValue)
                    {
                        return aggregatedValue;
                    }

                    return aggregatedValue + valueToAggregate;
                }

                private bool AreShippingAddressesEqual(Address address1, Address address2)
                {
                    if (address1 == null && address2 == null)
                    {
                        return true;
                    }

                    if (address1 == null || address2 == null)
                    {
                        return false;
                    }

                    return address1.RecordId == address2.RecordId;
                }
            }

            private const string AggregateSalesLinesQuantityKey = "Workflow.Receipt.AggregateSalesLinesQuantity";

            /// <summary>
            /// Aggregate a collection of sales lines.
            /// </summary>
            /// <param name="salesLines">The sales lines to aggregate.</param>
            /// <param name="context">The request context.</param>
            /// <returns>A collection of sales lines that have been aggregated.</returns>
            internal Collection<SalesLine> AggregateSalesLines(Collection<SalesLine> salesLines, RequestContext context)
            {
                if (salesLines.IsNullOrEmpty() || salesLines.Count == 1)
                {
                    return salesLines;
                }

                List<SalesLinePair> list = new List<SalesLinePair>();
                Collection<SalesLine> collection = new Collection<SalesLine>();
                bool valueOrDefault = context.Runtime.Configuration.GetSettingValue<bool>(AggregateSalesLinesQuantityKey).GetValueOrDefault(false);

                foreach (SalesLine salesLine in salesLines)
                {
                    if (ShouldAggregate(salesLine))
                    {
                        AggregateSalesLines(list, salesLine, valueOrDefault);
                    }
                    else
                    {
                        collection.Add(salesLine);
                    }
                }

                Collection<SalesLine> collection2 = new Collection<SalesLine>();
                foreach (SalesLinePair item in list)
                {
                    collection2.Add(item.AggregatedLine);
                }

                collection2.AddRange(collection);
                return collection2;
            }

            /// <summary>
            /// Aggregates a sales line to the existing aggregation result.
            /// </summary>
            /// <param name="pairs">The existing sales line pairs list which contains aggregation result.</param>
            /// <param name="salesLine">The sales line to aggregate.</param>
            /// <param name="isAggregateSalesLinesQuantity">AggregateSalesLinesQuantity configuration key.</param>
            private void AggregateSalesLines(List<SalesLinePair> pairs, SalesLine salesLine, bool isAggregateSalesLinesQuantity)
            {
                foreach (SalesLinePair pair in pairs)
                {
                    if (pair.CanAggregate(salesLine, isAggregateSalesLinesQuantity))
                    {
                        pair.Aggregate(salesLine);
                        return;
                    }
                }

                pairs.Add(new SalesLinePair(salesLine));
            }

            private bool ShouldAggregate(SalesLine line)
            {
                if (line.IsGiftCardLine || line.IsCustomerAccountDeposit || line.IsInvoiceLine || line.IsReturnByReceipt || line.IsVoided || !string.IsNullOrEmpty(line.LinkedParentLineId))
                {
                    return false;
                }

                if (line.Quantity < 0m || line.ReturnQuantity > 0m)
                {
                    return false;
                }

                return true;
            }
        }
    }
}
