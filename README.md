# **📦 VVendorSystem – PackBank Vendor Template System**

*A clean, modern vendor template system for ServUO.*

This system replaces old-school vendor templates, cloned items, and clunky gumps with something far simpler and far more powerful:

**A hidden “packbank” container that acts as the master template for any vendor using its name.**

Admins edit vendor stock by opening that container.
Vendors read template items from it.
Economy rules & regional multipliers apply automatically.

---

# **✨ CORE FEATURES**

### **✔ PackBank Containers (Vendor Packs)**

Each vendor pack is a **hidden, non-movable container** that stores *template items* for a vendor.

Each template entry uses:

* ItemID / Hue (visual)
* Metadata (wrapper item)

  * Price
  * MaxStock
  * ResourceType
  * InfiniteStock flag
  * AllowSellback flag

### **✔ Built-In Editor**

Simply drag REAL items into the pack.
They automatically convert into template entries (`VendorTemplateItem`).

### **✔ Easy Commands**

```
[addsimplevendor <PackName>
[editvvpack <PackName>
```

### **✔ Auto-Creation**

If a pack doesn't exist when referenced:

* `[addsimplevendor]` will **create it automatically**
* `[editvvpack]` will create and open it

### **✔ Vendor Region Controllers**

Each vendor becomes its own lightweight “region-in-a-box”:

* RegionName
* Category (Town, Dungeon, VendorHub, etc.)
* Per-resource multipliers
* Bounding box region for dynamic economy

Editable with:

```
[Double-click the region controller]
[editregion <vendor>  ❗ only if you want variations ❗]
```

### **✔ Global Economy Manager**

Each vendor checks:

* Pack template price
* Region multipliers
* Global multipliers

And dynamically adjusts prices.

---

# **📂 FILES INCLUDED**

```
VendorPackContainer.cs
VendorTemplateItem (inner class)
SimpleVendor.cs
AddSimpleVendor.cs
VendorRegionController.cs
EconomyManager.cs
RegionEditorGump.cs
ResourceType.cs
BoundingBoxPreview.cs (optional)
README.md
```

---

# **🚀 HOW TO USE THE SYSTEM (Step-by-Step Workflows)**

---

# **1️⃣ Create or Edit a Vendor Template (PackBank Container)**

### **Create a new template named “MageGuild”:**

```
[editvvpack MageGuild
```

If it doesn’t exist, it will be created automatically.

### What you’ll see:

A normal UO container (packbank style) with a blank background.

### Add items:

1. Drag a REAL item from your pack into the container.
2. It is **not added** as a real item.
3. Instead it becomes a **VendorTemplateItem**, holding:

   * ItemID
   * Hue
   * Template metadata
4. Customize metadata via:

   ```
   [props on the template item
   ```

   Properties include:

   * VendorPrice
   * MaxStock
   * InfiniteStock
   * ResourceType
   * AllowSellback

---

# **2️⃣ Spawn a Vendor Using a Template**

### **Command:**

```
[addsimplevendor MageGuild
```

If the template didn’t exist, it is created and can be edited with:

```
[editvvpack MageGuild
```

### Vendor behavior:

* Pulls template items from the packbank container
* Generates buy/sell lists
* Applies region multipliers
* Uses default stock rules or template max stock

---

# **3️⃣ Region & Economy Setup**

Every newly created vendor automatically creates its own region controller.

### To edit a vendor’s region economy:

```
Double-click the invisible region controller (it appears in the vendor’s backpack for GM)
```

OR use:

```
[props <controller>
```

### From the Region Editor Gump:

* Adjust category (Town/Dungeon/etc.)
* Set region-wide multipliers
* Set per-resource-type multipliers
* Adjust bounding box using targeting

Vendors inside the region:

* Auto-adjust their prices
* Auto-load correct multipliers

---

# **4️⃣ Editing Vendor Templates At Any Time**

Modify a pack at any time:

```
[editvvpack MageGuild
```

Any vendor linked to this pack instantly updates stock and pricing.

No respawn required.
No scripts to recompile.
No gump to rebuild.

---

# **🧪 Example Workflow: Creating a Mage Shop**

### **Step 1 — Create Pack**

```
[editvvpack MageGuild
```

### **Step 2 — Add Items**

Drag scrolls, reagents, potions, etc.

For each template item:

* Set `VendorPrice`
* Set `MaxStock`
* Set `ResourceType`

Example:

```
VendorPrice = 45
MaxStock = 999
ResourceType = Reagents
```

### **Step 3 — Spawn Vendor**

```
[addsimplevendor MageGuild
```

### **Step 4 — Tune Region Economy**

* Double-click controller
* Increase reagent multiplier to 1.25 for a “Magic District”
* Set bounding box to the building interior

Done.

---

# **⚒ Admin Tips & Best Practices**

### ✔ Separate packs by theme:

```
MageGuild
WarriorSupplies
AlchemyShop
TownGeneralStore
DungeonRareVendor
```

### ✔ Keep items neatly arranged inside the pack

The container UI allows visual grouping.

### ✔ Use props for fine control:

* Infinite stock for essentials
* MaxStock for rare items
* Category multipliers

### ✔ Clone packs easily:

```
[editvvpack AlchemyShop
Rename container inside props
Save as new pack
```

---

# **🧹 Clean-Up & Maintenance**

### To delete a packbank entirely:

1. Open it:

   ```
   [editvvpack MageGuild
   ```
2. Delete all template items
3. Delete the container through:

   ```
   [delete
   ```

   or `[props] → Delete`

Vendors referencing missing pack:

* Create default empty pack on next refresh.

---

# **🏁 Conclusion**

This system provides:

* Near-zero overhead
* Native drag & drop
* Perfect admin workflow
* Extremely clean vendor templates
* Fully dynamic regional economy
* No duplicate items
* No gumps to maintain
* Easiest vendor management ServUO currently allows

---

## TODO

* **GUI gump wrapper for pack editing**


