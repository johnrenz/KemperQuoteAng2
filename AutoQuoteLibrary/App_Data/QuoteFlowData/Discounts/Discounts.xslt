<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:xs="http://www.w3.org/2001/XMLSchema" exclude-result-prefixes="xs">
    <xsl:output method="xml" encoding="UTF-8" indent="yes" omit-xml-declaration="yes"/>
    <xsl:param name="state" />
    <xsl:template match="/">
        <Discounts>
            <xsl:variable name="discount_state">
                <xsl:choose>
                    <xsl:when test="count(/*/State[@abbrev=$state]/Discount) &gt; 0">
                        <xsl:value-of select="$state" />
                    </xsl:when>
                    <xsl:otherwise>
                        <xsl:value-of select="'default'" />
                    </xsl:otherwise>
                </xsl:choose>
            </xsl:variable>

            <xsl:for-each select="/*/State[@abbrev=$discount_state]/Discount">
                <xsl:sort select="@sort_order" data-type="number" />
                <xsl:variable name="id" select="@display" />
                <Discount ID="{$id}">
                    <Name>
                        <xsl:value-of select="@header" />
                    </Name>
                    <Description>
                        <xsl:value-of select="@description" />
                    </Description>
                </Discount>
            </xsl:for-each>
        </Discounts>
    </xsl:template>
</xsl:stylesheet>
