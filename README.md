# DEH-CATIA

The DEH CATIA is a WPF C# Application that makes use of the [Digital Engineering Hub Common library](https://github.com/RHEAGROUP/DEHP-Common),
which is available as Nuget package.

The DEH CATIA allows users to interactivly exchange data between models built with the [CATIA](https://www.3ds.com/products-services/catia/) software and a ECSS-E-TM-10-25A data source.

# License

The libraries contained in the DEH CATIA are provided to the community under the GNU Lesser General Public License. Because we make the software available with the LGPL, it can be used in both open source and proprietary software without being required to release the source code of your own components.

# Contributions

Contributions to the code-base are welcome. However, before we can accept your contributions we ask any contributor to sign the Contributor License Agreement (CLA) and send this digitaly signed to s.gerene@rheagroup.com. You can find the CLA's in the CLA folder.

# Build Instructions
The DEH CATIA adapter uses the COM interface of a running CATIA V5 client to interact with CATIA. If not referenced already, the below list of object libraries should be added through COM references for the projects to build successfully.

By default, the *CATIA V5 installation folder* is: "C:\Program Files\Dassault Systemes\B30\win_b64\code\bin\".

| Library Name | Path |
| --- | --- |
| CATIA V5 CATMATInterfaces | *{CATIA V5 installation folder}*\CATMatTypeLib.tlb |
| CATIA V5 CATRmaInterfaces | *{CATIA V5 installation folder}*\CATRmaTypeLib.tlb |
| CATIA V5 InfInterfaces | *{CATIA V5 installation folder}*\InfTypeLib.tlb |
| CATIA V5 KinematicsInterfaces | *{CATIA V5 installation folder}*\KinTypeLib.tlb |
| CATIA V5 KnowledgeInterfaces | *{CATIA V5 installation folder}*\CATMatTypeLib.tlb |
| CATIA V5 MecModInterfaces | *{CATIA V5 installation folder}*\MecModTypeLib.tlb |
| CATIA V5 PartInterfaces | *{CATIA V5 installation folder}*\PartTypeLib.tlb |
| CATIA V5 ProductStructureInterfaces | *{CATIA V5 installation folder}*\PSTypeLib.tlb |
| CATIA V5 SpaceAnalysisInterfaces | *{CATIA V5 installation folder}*\SPATypeLib.tlb |
