import { initializeOrderForm } from "./order-form.js";
import { initializeOrdersList } from "./orders-list.js";

const notice = document.querySelector("#notice");
let noticeTimer;

function showNotice(message, kind = "success") {
  clearTimeout(noticeTimer);
  notice.textContent = message;
  notice.dataset.kind = kind;
  notice.hidden = false;
  noticeTimer = setTimeout(() => { notice.hidden = true; }, 5000);
}

const refreshOrders = initializeOrdersList(showNotice);
initializeOrderForm(refreshOrders, showNotice);
refreshOrders().catch(error => showNotice(error.message, "error"));
