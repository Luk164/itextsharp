using System;
using System.Text;
using iTextSharp.text;
using iTextSharp.text.error_messages;

/*
 * $Id: BarcodeInter25.cs,v 1.5 2006/09/17 15:58:51 psoares33 Exp $
 *
 * Copyright 2002-2006 by Paulo Soares.
 *
 * The contents of this file are subject to the Mozilla Public License Version 1.1
 * (the "License"); you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at http://www.mozilla.org/MPL/
 *
 * Software distributed under the License is distributed on an "AS IS" basis,
 * WITHOUT WARRANTY OF ANY KIND, either express or implied. See the License
 * for the specific language governing rights and limitations under the License.
 *
 * The Original Code is 'iText, a free JAVA-PDF library'.
 *
 * The Initial Developer of the Original Code is Bruno Lowagie. Portions created by
 * the Initial Developer are Copyright (C) 1999, 2000, 2001, 2002 by Bruno Lowagie.
 * All Rights Reserved.
 * Co-Developer of the code is Paulo Soares. Portions created by the Co-Developer
 * are Copyright (C) 2000, 2001, 2002 by Paulo Soares. All Rights Reserved.
 *
 * Contributor(s): all the names of the contributors are added in the source code
 * where applicable.
 *
 * Alternatively, the contents of this file may be used under the terms of the
 * LGPL license (the "GNU LIBRARY GENERAL PUBLIC LICENSE"), in which case the
 * provisions of LGPL are applicable instead of those above.  If you wish to
 * allow use of your version of this file only under the terms of the LGPL
 * License and not to allow others to use your version of this file under
 * the MPL, indicate your decision by deleting the provisions above and
 * replace them with the notice and other provisions required by the LGPL.
 * If you do not delete the provisions above, a recipient may use your version
 * of this file under either the MPL or the GNU LIBRARY GENERAL PUBLIC LICENSE.
 *
 * This library is free software; you can redistribute it and/or modify it
 * under the terms of the MPL as stated above or under the terms of the GNU
 * Library General Public License as published by the Free Software Foundation;
 * either version 2 of the License, or any later version.
 *
 * This library is distributed in the hope that it will be useful, but WITHOUT
 * ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
 * FOR A PARTICULAR PURPOSE. See the GNU Library general Public License for more
 * details.
 *
 * If you didn't download this code from the following link, you should check if
 * you aren't using an obsolete version:
 * http://www.lowagie.com/iText/
 */
namespace iTextSharp.text.pdf {
    /** Implements the code interleaved 2 of 5. The text can include
     * non numeric characters that are printed but do not generate bars.
     * The default parameters are:
     * <pre>
     *x = 0.8f;
     *n = 2;
     *font = BaseFont.CreateFont("Helvetica", "winansi", false);
     *size = 8;
     *baseline = size;
     *barHeight = size * 3;
     *textint= Element.ALIGN_CENTER;
     *generateChecksum = false;
     *checksumText = false;
     * </pre>
     *
     * @author Paulo Soares (psoares@consiste.pt)
     */
    public class BarcodeInter25 : Barcode {

        /** The bars to generate the code.
         */    
        private static readonly byte[][] BARS = {
         new byte[] {0,0,1,1,0},
         new byte[] {1,0,0,0,1},
         new byte[] {0,1,0,0,1},
         new byte[] {1,1,0,0,0},
         new byte[] {0,0,1,0,1},
         new byte[] {1,0,1,0,0},
         new byte[] {0,1,1,0,0},
         new byte[] {0,0,0,1,1},
         new byte[] {1,0,0,1,0},
         new byte[] {0,1,0,1,0}
    };

        /** Creates new BarcodeInter25 */
        public BarcodeInter25() {
            x = 0.8f;
            n = 2;
            font = BaseFont.CreateFont("Helvetica", "winansi", false);
            size = 8;
            baseline = size;
            barHeight = size * 3;
            textAlignment = Element.ALIGN_CENTER;
            generateChecksum = false;
            checksumText = false;
        }
    
        /** Deletes all the non numeric characters from <CODE>text</CODE>.
         * @param text the text
         * @return a <CODE>string</CODE> with only numeric characters
         */    
        public static string KeepNumbers(string text) {
            StringBuilder sb = new StringBuilder();
            for (int k = 0; k < text.Length; ++k) {
                char c = text[k];
                if (c >= '0' && c <= '9')
                    sb.Append(c);
            }
            return sb.ToString();
        }
    
        /** Calculates the checksum.
         * @param text the numeric text
         * @return the checksum
         */    
        public static char GetChecksum(string text) {
            int mul = 3;
            int total = 0;
            for (int k = text.Length - 1; k >= 0; --k) {
                int n = text[k] - '0';
                total += mul * n;
                mul ^= 2;
            }
            return (char)(((10 - (total % 10)) % 10) + '0');
        }

        /** Creates the bars for the barcode.
         * @param text the text. It can contain non numeric characters
         * @return the barcode
         */    
        public static byte[] GetBarsInter25(string text) {
            text = KeepNumbers(text);
            if ((text.Length & 1) != 0)
                throw new ArgumentException(MessageLocalization.GetComposedMessage("the.text.length.must.be.even"));
            byte[] bars = new byte[text.Length * 5 + 7];
            int pb = 0;
            bars[pb++] = 0;
            bars[pb++] = 0;
            bars[pb++] = 0;
            bars[pb++] = 0;
            int len = text.Length / 2;
            for (int k = 0; k < len; ++k) {
                int c1 = text[k * 2] - '0';
                int c2 = text[k * 2 + 1] - '0';
                byte[] b1 = BARS[c1];
                byte[] b2 = BARS[c2];
                for (int j = 0; j < 5; ++j) {
                    bars[pb++] = b1[j];
                    bars[pb++] = b2[j];
                }
            }
            bars[pb++] = 1;
            bars[pb++] = 0;
            bars[pb++] = 0;
            return bars;
        }

        /** Gets the maximum area that the barcode and the text, if
         * any, will occupy. The lower left corner is always (0, 0).
         * @return the size the barcode occupies.
         */    
        public override Rectangle BarcodeSize {
            get {
                float fontX = 0;
                float fontY = 0;
                if (font != null) {
                    if (baseline > 0)
                        fontY = baseline - font.GetFontDescriptor(BaseFont.DESCENT, size);
                    else
                        fontY = -baseline + size;
                    string fullCode = code;
                    if (generateChecksum && checksumText)
                        fullCode += GetChecksum(fullCode);
                    fontX = font.GetWidthPoint(altText != null ? altText : fullCode, size);
                }
                string fCode = KeepNumbers(code);
                int len = fCode.Length;
                if (generateChecksum)
                    ++len;
                float fullWidth = len * (3 * x + 2 * x * n) + (6 + n ) * x;
                fullWidth = Math.Max(fullWidth, fontX);
                float fullHeight = barHeight + fontY;
                return new Rectangle(fullWidth, fullHeight);
            }
        }
    
        /** Places the barcode in a <CODE>PdfContentByte</CODE>. The
         * barcode is always placed at coodinates (0, 0). Use the
         * translation matrix to move it elsewhere.<p>
         * The bars and text are written in the following colors:<p>
         * <P><TABLE BORDER=1>
         * <TR>
         *    <TH><P><CODE>barColor</CODE></TH>
         *    <TH><P><CODE>textColor</CODE></TH>
         *    <TH><P>Result</TH>
         *    </TR>
         * <TR>
         *    <TD><P><CODE>null</CODE></TD>
         *    <TD><P><CODE>null</CODE></TD>
         *    <TD><P>bars and text painted with current fill color</TD>
         *    </TR>
         * <TR>
         *    <TD><P><CODE>barColor</CODE></TD>
         *    <TD><P><CODE>null</CODE></TD>
         *    <TD><P>bars and text painted with <CODE>barColor</CODE></TD>
         *    </TR>
         * <TR>
         *    <TD><P><CODE>null</CODE></TD>
         *    <TD><P><CODE>textColor</CODE></TD>
         *    <TD><P>bars painted with current color<br>text painted with <CODE>textColor</CODE></TD>
         *    </TR>
         * <TR>
         *    <TD><P><CODE>barColor</CODE></TD>
         *    <TD><P><CODE>textColor</CODE></TD>
         *    <TD><P>bars painted with <CODE>barColor</CODE><br>text painted with <CODE>textColor</CODE></TD>
         *    </TR>
         * </TABLE>
         * @param cb the <CODE>PdfContentByte</CODE> where the barcode will be placed
         * @param barColor the color of the bars. It can be <CODE>null</CODE>
         * @param textColor the color of the text. It can be <CODE>null</CODE>
         * @return the dimensions the barcode occupies
         */    
        public override Rectangle PlaceBarcode(PdfContentByte cb, Color barColor, Color textColor) {
            string fullCode = code;
            float fontX = 0;
            if (font != null) {
                if (generateChecksum && checksumText)
                    fullCode += GetChecksum(fullCode);
                fontX = font.GetWidthPoint(fullCode = altText != null ? altText : fullCode, size);
            }
            string bCode = KeepNumbers(code);
            if (generateChecksum)
                bCode += GetChecksum(bCode);
            int len = bCode.Length;
            float fullWidth = len * (3 * x + 2 * x * n) + (6 + n ) * x;
            float barStartX = 0;
            float textStartX = 0;
            switch (textAlignment) {
                case Element.ALIGN_LEFT:
                    break;
                case Element.ALIGN_RIGHT:
                    if (fontX > fullWidth)
                        barStartX = fontX - fullWidth;
                    else
                        textStartX = fullWidth - fontX;
                    break;
                default:
                    if (fontX > fullWidth)
                        barStartX = (fontX - fullWidth) / 2;
                    else
                        textStartX = (fullWidth - fontX) / 2;
                    break;
            }
            float barStartY = 0;
            float textStartY = 0;
            if (font != null) {
                if (baseline <= 0)
                    textStartY = barHeight - baseline;
                else {
                    textStartY = -font.GetFontDescriptor(BaseFont.DESCENT, size);
                    barStartY = textStartY + baseline;
                }
            }
            byte[] bars = GetBarsInter25(bCode);
            bool print = true;
            if (barColor != null)
                cb.SetColorFill(barColor);
            for (int k = 0; k < bars.Length; ++k) {
                float w = (bars[k] == 0 ? x : x * n);
                if (print)
                    cb.Rectangle(barStartX, barStartY, w - inkSpreading, barHeight);
                print = !print;
                barStartX += w;
            }
            cb.Fill();
            if (font != null) {
                if (textColor != null)
                    cb.SetColorFill(textColor);
                cb.BeginText();
                cb.SetFontAndSize(font, size);
                cb.SetTextMatrix(textStartX, textStartY);
                cb.ShowText(fullCode);
                cb.EndText();
            }
            return this.BarcodeSize;
        }   

        public override System.Drawing.Image CreateDrawingImage(System.Drawing.Color foreground, System.Drawing.Color background) {
            String bCode = KeepNumbers(code);
            if (generateChecksum)
                bCode += GetChecksum(bCode);
            int len = bCode.Length;
            int nn = (int)n;
            int fullWidth = len * (3 + 2 * nn) + (6 + nn );
            byte[] bars = GetBarsInter25(bCode);
            int height = (int)barHeight;
            System.Drawing.Bitmap bmp = new System.Drawing.Bitmap(fullWidth, height);
            for (int h = 0; h < height; ++h) {
                bool print = true;
                int ptr = 0;
                for (int k = 0; k < bars.Length; ++k) {
                    int w = (bars[k] == 0 ? 1 : nn);
                    System.Drawing.Color c = background;
                    if (print)
                        c = foreground;
                    print = !print;
                    for (int j = 0; j < w; ++j)
                        bmp.SetPixel(ptr++, h, c);
                }
            }
            return bmp;
        }
    }
}
