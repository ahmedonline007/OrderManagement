import { ordersApi } from "./api.js";

const money = amount => new Intl.NumberFormat(undefined, { style: "currency", currency: "USD" }).format(amount);

export function initializeOrdersList(showNotice) {
  const filters = { page: 1, pageSize: 10, sortBy: "date", descending: true, search: "" };
  const list = document.querySelector("#orders-list");
  const emptyState = document.querySelector("#empty-state");
  const deleteDialog = document.querySelector("#delete-dialog");
  const confirmDelete = document.querySelector("#confirm-delete");
  let pendingDeleteId = null;
  let searchTimer;

  function addText(parent, tag, className, value) {
    const element = document.createElement(tag);
    if (className) element.className = className;
    element.textContent = value;
    parent.append(element);
    return element;
  }

  function renderOrder(order) {
    const card = document.createElement("article");
    card.className = "order-card";
    card.dataset.orderId = order.id;

    const customer = document.createElement("div");
    addText(customer, "div", "order-customer", order.customer.name);
    addText(customer, "div", "order-email", order.customer.email);
    card.append(customer);

    const products = document.createElement("div");
    products.className = "order-products";
    for (const item of order.items) {
      addText(products, "span", "", `${item.productName} · ${item.quantity} × ${money(item.unitPrice)}`);
    }
    card.append(products);

    const summary = document.createElement("div");
    addText(summary, "div", "order-total", money(order.total));
    if (order.discountAmount > 0) {
      addText(summary, "div", "order-date", `${order.discountCode} discount: −${money(order.discountAmount)}`);
    }
    addText(summary, "div", "order-date", new Date(order.createdAtUtc).toLocaleString());
    card.append(summary);

    const actions = document.createElement("div");
    actions.className = "order-actions";
    const deleteButton = document.createElement("button");
    deleteButton.type = "button";
    deleteButton.className = "button button-secondary";
    deleteButton.dataset.deleteId = order.id;
    deleteButton.textContent = "Delete";
    actions.append(deleteButton);
    card.append(actions);
    return card;
  }

  async function load() {
    const result = await ordersApi.list(filters);
    list.replaceChildren(...result.items.map(renderOrder));
    emptyState.hidden = result.items.length > 0;
    document.querySelector("#result-count").textContent =
      `${result.totalCount} ${result.totalCount === 1 ? "order" : "orders"}`;
    const totalPages = Math.max(1, Math.ceil(result.totalCount / result.pageSize));
    document.querySelector("#page-label").textContent = `Page ${result.page} of ${totalPages}`;
    document.querySelector("#previous-page").disabled = result.page <= 1;
    document.querySelector("#next-page").disabled = result.page >= totalPages;
  }

  list.addEventListener("click", event => {
    const button = event.target.closest("[data-delete-id]");
    if (!button) return;
    pendingDeleteId = button.dataset.deleteId;
    deleteDialog.showModal();
  });
  confirmDelete.addEventListener("click", async () => {
    if (!pendingDeleteId) return;
    confirmDelete.disabled = true;
    try {
      await ordersApi.delete(pendingDeleteId);
      deleteDialog.close();
      pendingDeleteId = null;
      showNotice("Order deleted.");
      if (list.childElementCount === 1 && filters.page > 1) filters.page -= 1;
      await load();
    } catch (error) {
      deleteDialog.close();
      showNotice(error.message, "error");
    } finally {
      confirmDelete.disabled = false;
    }
  });

  document.querySelector("#order-search").addEventListener("input", event => {
    clearTimeout(searchTimer);
    searchTimer = setTimeout(() => {
      filters.search = event.target.value.trim();
      filters.page = 1;
      load().catch(error => showNotice(error.message, "error"));
    }, 300);
  });
  document.querySelector("#order-sort").addEventListener("change", event => {
    filters.sortBy = event.target.value;
    filters.page = 1;
    load().catch(error => showNotice(error.message, "error"));
  });
  document.querySelector("#order-descending").addEventListener("change", event => {
    filters.descending = event.target.checked;
    filters.page = 1;
    load().catch(error => showNotice(error.message, "error"));
  });
  document.querySelector("#previous-page").addEventListener("click", () => {
    filters.page -= 1;
    load().catch(error => showNotice(error.message, "error"));
  });
  document.querySelector("#next-page").addEventListener("click", () => {
    filters.page += 1;
    load().catch(error => showNotice(error.message, "error"));
  });

  return load;
}
