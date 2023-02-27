using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CopyGroupPlugin
{
    [TransactionAttribute(TransactionMode.Manual)]
    public class CopyGroup : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            try
            {
                UIDocument uIDocument = commandData.Application.ActiveUIDocument;
                Document document = uIDocument.Document;

                GroupSelectionFilter selectionFilter = new  GroupSelectionFilter();

                Reference reference = uIDocument.Selection.PickObject(ObjectType.Element, selectionFilter, "Выберите элементы");
                Element element = document.GetElement(reference);
                Group group = element as Group;
                
                XYZ groupCenter =GetElementCenter(group);
                Room room = GetRoomByPoint(document,groupCenter);
                XYZ roomCenter1 = GetElementCenter(room);
                XYZ offset = groupCenter - roomCenter1;


                XYZ point = uIDocument.Selection.PickPoint("Выберите точку");
                Room room2 = GetRoomByPoint(document, point);
                XYZ roomCenter2 = GetElementCenter(room2);
                XYZ groupPointPlacement = roomCenter2 + offset;


                Transaction transaction = new Transaction(document);
                transaction.Start("Копирование группы");
                document.Create.PlaceGroup(groupPointPlacement, group.GroupType);
                transaction.Commit();
            }
            catch(Autodesk.Revit.Exceptions.OperationCanceledException)
            {
                return Result.Cancelled;
            }
            catch(Exception ex)
            {
                message = ex.Message;   
                return Result.Failed;
            }

            return Result.Succeeded;
        }
        public XYZ GetElementCenter(Element element)
        {
            BoundingBoxXYZ boundingBox = element.get_BoundingBox(null);
            return (boundingBox.Max + boundingBox.Min) / 2;
        }
        public Room GetRoomByPoint(Document document,XYZ point)
        {
            FilteredElementCollector collector = new FilteredElementCollector(document);
            collector.OfCategory(BuiltInCategory.OST_Rooms);
            foreach (Element element in collector)
            {
                Room room = element as Room;    
                if (room != null)
                {
                    if (room.IsPointInRoom(point))
                    {
                        return room;
                    }
                }  
            }
            return null;
        }
    }
    public class GroupSelectionFilter : ISelectionFilter
    {
        public bool AllowElement(Element elem)
        {
          if (elem.Category.Id.IntegerValue == (int) BuiltInCategory.OST_IOSModelGroups)
                return true;
          else
                return false;
        }

        public bool AllowReference(Reference reference, XYZ position)
        {
            return false;
        }
    }
    
}
