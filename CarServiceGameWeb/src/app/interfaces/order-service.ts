import { Observable } from "rxjs/Observable";
import { Order } from "../model/order";
import { RepairProcess } from "../model/repair-process";

export interface IOrderService {
    getHistoryOrders(): Array<RepairProcess>;
    getHistoryOrdersFromAPI();
    updateOrders();
    getOrders(): Array<Order>;
}