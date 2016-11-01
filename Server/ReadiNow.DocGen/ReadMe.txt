ReadiNow.DocGen
~~~~~~~~~~~~~~~

Provides services for generating Word documents from Word templates.

These can be invoked directly via console actions; or run as part of a workflow.

Dependencies need to be passed around into data classes, so they are passed in a single container class: ExternalServices.

Assembly aliases
~~~~~~~~~~~~~~~~
Assembly aliases are used in this project.
The EDC.ReadiNow.Common assembly is assigned the EdcReadinowCommon alias, rather than global.
The motivation for this is to make it *impossible* to accidentally reference any type from that assembly,
thereby strictly enforcing the use of dependency injection.

~~~~~~~~~~~~~~~~~~~~
Sample XML structure of Word documents...


<?xml version="1.0" encoding="utf-16"?>
<w:body xmlns:w="http://schemas.openxmlformats.org/wordprocessingml/2006/main">
  <w:p w:rsidRPr="002A37D4" w:rsidR="00ED5A7E" w:rsidP="002A37D4" w:rsidRDefault="002A37D4">
    <w:r>
      <w:t xml:space="preserve">Hello </w:t>
    </w:r>
    <w:fldSimple w:instr=" MERGEFIELD  blah  \* MERGEFORMAT ">
      <w:r>
        <w:rPr>
          <w:noProof />
        </w:rPr>
        <w:t>«blah»</w:t>
      </w:r>
    </w:fldSimple>
    <w:bookmarkStart w:name="_GoBack" w:id="0" />
    <w:bookmarkEnd w:id="0" />
  </w:p>
  <w:sectPr w:rsidRPr="002A37D4" w:rsidR="00ED5A7E">
    <w:pgSz w:w="11906" w:h="16838" />
    <w:pgMar w:top="1440" w:right="1440" w:bottom="1440" w:left="1440" w:header="708" w:footer="708" w:gutter="0" />
    <w:cols w:space="708" />
    <w:docGrid w:linePitch="360" />
  </w:sectPr>
</w:body>


<?xml version="1.0" encoding="utf-16"?>
<w:body xmlns:w="http://schemas.openxmlformats.org/wordprocessingml/2006/main">
  <w:p w:rsidRPr="002A37D4" w:rsidR="00ED5A7E" w:rsidP="002A37D4" w:rsidRDefault="002A37D4">
    <w:bookmarkStart w:name="_GoBack" w:id="0" />
    <w:r w:rsidRPr="004E250C">
      <w:rPr>
        <w:b />
      </w:rPr>
      <w:t xml:space="preserve">Hello </w:t>
    </w:r>
    <w:r w:rsidRPr="004E250C" w:rsidR="004E250C">
      <w:rPr>
        <w:b />
      </w:rPr>
      <w:fldChar w:fldCharType="begin" />
    </w:r>
    <w:r w:rsidRPr="004E250C" w:rsidR="004E250C">
      <w:rPr>
        <w:b />
      </w:rPr>
      <w:instrText xml:space="preserve"> MERGEFIELD  blah  \* MERGEFORMAT </w:instrText>
    </w:r>
    <w:r w:rsidRPr="004E250C" w:rsidR="004E250C">
      <w:rPr>
        <w:b />
      </w:rPr>
      <w:fldChar w:fldCharType="separate" />
    </w:r>
    <w:r w:rsidRPr="004E250C">
      <w:rPr>
        <w:b />
        <w:noProof />
      </w:rPr>
      <w:t>«blah»</w:t>
    </w:r>
    <w:r w:rsidRPr="004E250C" w:rsidR="004E250C">
      <w:rPr>
        <w:b />
        <w:noProof />
      </w:rPr>
      <w:fldChar w:fldCharType="end" />
    </w:r>
    <w:bookmarkEnd w:id="0" />
  </w:p>
  <w:sectPr w:rsidRPr="002A37D4" w:rsidR="00ED5A7E">
    <w:pgSz w:w="11906" w:h="16838" />
    <w:pgMar w:top="1440" w:right="1440" w:bottom="1440" w:left="1440" w:header="708" w:footer="708" w:gutter="0" />
    <w:cols w:space="708" />
    <w:docGrid w:linePitch="360" />
  </w:sectPr>
</w:body>