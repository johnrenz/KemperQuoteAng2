<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:xs="http://www.w3.org/2001/XMLSchema" exclude-result-prefixes="xs">
    <xsl:output method="xml" encoding="UTF-8" indent="yes" omit-xml-declaration="yes"/>
    <xsl:param name="hidedisccov" select="/.."/>
    <xsl:template match="/">
        <xsl:variable name="var1_instance_DiscountState" select="."/>
        <Discounts>
            <xsl:for-each select="$var1_instance_DiscountState/State">
                <xsl:variable name="var2_State" select="."/>
                <xsl:if test="string(not(($hidedisccov = 'TRUE'))) != 'false'">
                <DiscountCoverages>
                    <xsl:for-each select="$var2_State/Discount">
                        <xsl:sort select="@sort_order" data-type="number"/>
                        <xsl:variable name="var4_Discount" select="."/>
                            <Discount>
                                <Name>
                                    <xsl:value-of select="string($var4_Discount/@header)"/>
                                </Name>
                                <ImageFileName>
                                    <xsl:value-of select="string($var4_Discount/@image_url)"/>
                                </ImageFileName>
                                <Description>
                                    <xsl:value-of select="string($var4_Discount/@sub_header)"/>
                                </Description>
                                <ExpandedDesc>
                                    <xsl:value-of select="string($var4_Discount/@faq)"/>
                                </ExpandedDesc>
                                <CanBeDeleted>
                                    <xsl:value-of select="(('0' != normalize-space($var4_Discount/@selectable)) and ('false' != normalize-space($var4_Discount/@selectable)))"/>
                                </CanBeDeleted>
                                <Purchased>
                                    <xsl:value-of select="(('0' != normalize-space($var4_Discount/@selected)) and ('false' != normalize-space($var4_Discount/@selected)))"/>
                                </Purchased>
                            </Discount>
                    </xsl:for-each>
                </DiscountCoverages>
                </xsl:if>
            </xsl:for-each>
            <xsl:for-each select="$var1_instance_DiscountState/State">
                <xsl:variable name="var6_State" select="."/>
                <DiscountPremiums>
                    <xsl:for-each select="$var6_State/Discount">
                        <xsl:sort select="@sort_order" data-type="number"/>
                        <xsl:variable name="var8_Discount" select="."/>
                            <Premium>
                                <Name>
                                    <xsl:value-of select="string($var8_Discount/@header)"/>
                                </Name>
                                <Amount>
                                    <xsl:value-of select="number(string(string($var8_Discount/@prem)))"/>
                                </Amount>
                            </Premium>
                    </xsl:for-each>
                </DiscountPremiums>
            </xsl:for-each>
        </Discounts>
    </xsl:template>
</xsl:stylesheet>
