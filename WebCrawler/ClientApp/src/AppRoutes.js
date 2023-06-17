import { Home } from "./components/Home";
import { Records } from "./components/PaginatedView";
import { ViewGraph } from "./components/ViewGraph";

const AppRoutes = [
  {
    index: true,
    element: <Home />,
  },
  {
    path: "/Graph",
    element: <ViewGraph />,
  },
];

export default AppRoutes;
