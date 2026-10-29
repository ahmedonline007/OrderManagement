async function request(url, options = {}) {
  const response = await fetch(url, {
    ...options,
    headers: {
      Accept: "application/json",
      ...(options.body ? { "Content-Type": "application/json" } : {}),
      ...options.headers
    }
  });

  if (!response.ok) {
    const body = await response.json().catch(() => null);
    const validationMessages = body?.errors
      ? Object.values(body.errors).flat().join(" ")
      : body?.detail || body?.title;
    throw new Error(validationMessages || `Request failed (${response.status}).`);
  }

  return response.status === 204 ? null : response.json();
}

export const ordersApi = {
  list(filters) {
    const params = new URLSearchParams({
      page: String(filters.page),
      pageSize: String(filters.pageSize),
      sortBy: filters.sortBy,
      descending: String(filters.descending)
    });
    if (filters.search) params.set("search", filters.search);
    return request(`/api/orders?${params}`);
  },
  create(order) {
    return request("/api/orders", { method: "POST", body: JSON.stringify(order) });
  },
  delete(id) {
    return request(`/api/orders/${encodeURIComponent(id)}`, { method: "DELETE" });
  },
  discounts() {
    return request("/api/orders/discounts");
  },
  previewDiscount(strategy, subtotal) {
    return request("/api/orders/discounts/preview", {
      method: "POST",
      body: JSON.stringify({ strategy, subtotal })
    });
  }
};
