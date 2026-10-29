import { ordersApi } from "./api.js";

const money = amount => new Intl.NumberFormat(undefined, { style: "currency", currency: "USD" }).format(amount);

export function initializeOrderForm(onCreated, showNotice) {
  const form = document.querySelector("#order-form");
  const itemList = document.querySelector("#items");
  const template = document.querySelector("#item-template");
  const strategy = document.querySelector("#discount-strategy");
  const formError = document.querySelector("#form-error");
  const submitButton = form.querySelector('[type="submit"]');

  function addItem() {
    itemList.append(template.content.cloneNode(true));
    updateDiscount();
  }

  function currentItems() {
    return [...itemList.querySelectorAll(".item-row")].map(row => ({
      productName: row.querySelector(".item-name").value.trim(),
      quantity: Number(row.querySelector(".item-quantity").value),
      unitPrice: Number(row.querySelector(".item-price").value)
    }));
  }

  function subtotal() {
    return currentItems().reduce((sum, item) => sum + item.quantity * item.unitPrice, 0);
  }

  async function updateDiscount() {
    const amount = subtotal();
    const label = document.querySelector("#discount-label");
    const total = document.querySelector("#discount-total");
    if (!strategy.value || !Number.isFinite(amount) || amount <= 0) {
      label.textContent = `Subtotal: ${money(Math.max(amount || 0, 0))}`;
      total.textContent = `Total: ${money(Math.max(amount || 0, 0))}`;
      return;
    }

    try {
      const preview = await ordersApi.previewDiscount(strategy.value, amount);
      label.textContent = `Subtotal: ${money(preview.subtotal)} · Discount: ${money(preview.discount)}`;
      total.textContent = `Total: ${money(preview.total)}`;
    } catch (error) {
      formError.textContent = error.message;
      formError.hidden = false;
    }
  }

  async function loadDiscounts() {
    const options = await ordersApi.discounts();
    for (const option of options) {
      const element = document.createElement("option");
      element.value = option.code;
      element.textContent = option.name;
      strategy.append(element);
    }
  }

  document.querySelector("#add-item").addEventListener("click", addItem);
  itemList.addEventListener("click", event => {
    const removeButton = event.target.closest(".remove-item");
    if (!removeButton) return;
    if (itemList.querySelectorAll(".item-row").length <= 1) {
      showNotice("An order needs at least one item.", "error");
      return;
    }
    removeButton.closest(".item-row").remove();
    updateDiscount();
  });
  itemList.addEventListener("input", updateDiscount);
  strategy.addEventListener("change", updateDiscount);
  form.addEventListener("submit", async event => {
    event.preventDefault();
    formError.hidden = true;
    formError.textContent = "";
    if (!form.reportValidity()) return;

    const items = currentItems();
    if (items.some(item => !item.productName || item.quantity < 1 || item.unitPrice <= 0)) {
      formError.textContent = "Enter a product, a positive quantity, and a unit price for every item.";
      formError.hidden = false;
      return;
    }

    submitButton.disabled = true;
    try {
      await ordersApi.create({
        customerName: form.elements.customerName.value.trim(),
        customerEmail: form.elements.customerEmail.value.trim(),
        discountStrategy: strategy.value || null,
        items
      });
      form.reset();
      itemList.replaceChildren();
      addItem();
      addItem();
      strategy.value = "";
      updateDiscount();
      showNotice("Order created successfully.");
      await onCreated();
    } catch (error) {
      formError.textContent = error.message;
      formError.hidden = false;
    } finally {
      submitButton.disabled = false;
    }
  });

  addItem();
  addItem();
  loadDiscounts().catch(error => {
    formError.textContent = `Could not load discount strategies: ${error.message}`;
    formError.hidden = false;
  });
}
