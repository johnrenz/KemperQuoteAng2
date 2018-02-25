<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xmlns:xs="http://www.w3.org/2001/XMLSchema" exclude-result-prefixes="xs xsi xsl">
    <xsl:output method="xml" encoding="UTF-8" indent="yes"/>
    <xsl:template match="/State">
        <Coverage>
            <xsl:variable name="Vvar1_firstSource" select="."/>
            <xsl:for-each select="Coverages">
                <xsl:for-each select="Coverage">
                    <xsl:variable name="Vvar93_Coverage" select="."/>
                    <xsl:variable name="ExistsResult">
                        <xsl:for-each select="@cov_type">
                            <xsl:variable name="Vvar97_cov_type_string" select="string(.)"/>
                            <xsl:for-each select="$Vvar1_firstSource/@abbrev">
                                <xsl:variable name="Vvar102_abbrev_string" select="string(.)"/>
                                <xsl:if test="(($Vvar97_cov_type_string) = ('FPB')) and (($Vvar102_abbrev_string) = ('PA'))">
                                    <xsl:value-of select="true()"/>
                                </xsl:if>
                            </xsl:for-each>
                        </xsl:for-each>
                    </xsl:variable>
                    <xsl:variable name="Vvar95_exists" select="string-length( $ExistsResult )&gt;0"/>
                    <xsl:if test="string($Vvar95_exists)='true' or string($Vvar95_exists)='1'">
                        <xsl:for-each select="$Vvar1_firstSource/@abbrev">
                            <xsl:variable name="Vvar108_abbrev_string" select="string(.)"/>
                            <xsl:for-each select="$Vvar93_Coverage/@cov_type">
                                <xsl:variable name="Vvar111_cov_type_string" select="string(.)"/>
                                <CovCode>
                                    <xsl:value-of select="concat($Vvar108_abbrev_string, $Vvar111_cov_type_string)"/>
                                </CovCode>
                            </xsl:for-each>
                        </xsl:for-each>
                        <QuoteProperty>
                            <xsl:value-of select="''"/>
                        </QuoteProperty>
                        <xsl:for-each select="@question_id">
                            <xsl:variable name="Vvar120_question_id_string" select="string(.)"/>
                            <WebQuestionID>
                                <xsl:value-of select="$Vvar120_question_id_string"/>
                            </WebQuestionID>
                        </xsl:for-each>
                        <SuppressRendering>
                            <xsl:value-of select="'False'"/>
                        </SuppressRendering>
                        <VehIndex>
                            <xsl:value-of select="number('-1')"/>
                        </VehIndex>
                        <HelpText/>
                        <Invalid>
                            <xsl:value-of select="'False'"/>
                        </Invalid>
                        <Offered>
                            <xsl:value-of select="'True'"/>
                        </Offered>
                        <xsl:for-each select="$Vvar1_firstSource/@abbrev">
                            <xsl:variable name="Vvar131_abbrev_string" select="string(.)"/>
                            <xsl:for-each select="$Vvar93_Coverage/@cov_type">
                                <xsl:variable name="Vvar134_cov_type_string" select="string(.)"/>
                                <Abbrev>
                                    <xsl:value-of select="concat($Vvar131_abbrev_string, $Vvar134_cov_type_string)"/>
                                </Abbrev>
                            </xsl:for-each>
                        </xsl:for-each>
                        <xsl:for-each select="@desc">
                            <xsl:variable name="Vvar141_desc_string" select="string(.)"/>
                            <Desc>
                                <xsl:value-of select="$Vvar141_desc_string"/>
                            </Desc>
                        </xsl:for-each>
                        <CovOption>
                            <xsl:value-of select="'False'"/>
                        </CovOption>
                        <CovMessage/>
                        <xsl:for-each select="@input_type">
                            <xsl:variable name="Vvar148_input_type_string" select="string(.)"/>
                            <CovInputType>
                                <xsl:value-of select="$Vvar148_input_type_string"/>
                            </CovInputType>
                        </xsl:for-each>
                        <xsl:for-each select="Options">
                            <Limits>
                                <xsl:for-each select="Option">
                                    <Limit>
                                        <xsl:for-each select="@opt_key">
                                            <xsl:variable name="Vvar156_opt_key_string" select="string(.)"/>
                                            <Value>
                                                <xsl:value-of select="$Vvar156_opt_key_string"/>
                                            </Value>
                                        </xsl:for-each>
                                        <Abbrev>
                                            <xsl:value-of select="''"/>
                                        </Abbrev>
                                        <xsl:for-each select="@opt_value">
                                            <xsl:variable name="Vvar162_opt_value_string" select="string(.)"/>
                                            <Caption>
                                                <xsl:value-of select="$Vvar162_opt_value_string"/>
                                            </Caption>
                                        </xsl:for-each>
                                        <SortOrder>
                                            <xsl:value-of select="number('')"/>
                                        </SortOrder>
                                        <IsNoCov>
                                            <xsl:value-of select="'False'"/>
                                        </IsNoCov>
                                        <Tag/>
                                    </Limit>
                                </xsl:for-each>
                                <SelectedLimitValue>
                                    <xsl:value-of select="'50~0~0~0~1~0'"/>
                                </SelectedLimitValue>
                            </Limits>
                        </xsl:for-each>
                        <xsl:for-each select="@faq_text">
                            <xsl:variable name="Vvar173_faq_text_string" select="string(.)"/>
                            <FAQText>
                                <xsl:value-of select="$Vvar173_faq_text_string"/>
                            </FAQText>
                        </xsl:for-each>
                        <IsForceEditOnChange>
                            <xsl:value-of select="'False'"/>
                        </IsForceEditOnChange>
                    </xsl:if>
                </xsl:for-each>
            </xsl:for-each>
        </Coverage>
    </xsl:template>
</xsl:stylesheet>

