import React, { Component } from "react";
import { useLocation } from "react-router";
import { NodeInfo } from "./NodeInfo";
import * as d3 from "d3";
import { CreateWebsiteRecordModalWindow } from "./CreateWebsiteRecordModalWindow";

export const ViewGraph = (props) => {
  const location = useLocation();
  return (
    <ViewGraphInternal
      ids={location.state.ids}
      height="600px"
      width="100%"
      {...props}
    />
  );
};

class ViewGraphInternal extends Component {
  constructor(props) {
    super(props);
    this.state = {
      loading: true,
      currentlySelectedNodeInfo: [],
    };
  }

  renderGraph(graph) {
    const nodes = graph.nodes.map((node) => Object.create(node));
    const links = graph.links.map((link) => Object.create(link));

    const svg = d3
      .select("svg")
      .attr("width", this.props.width)
      .attr("height", this.props.height);

    const simulation = d3
      .forceSimulation()
      .force("charge", d3.forceManyBody().strength(-20))
      .force(
        "center",
        d3.forceCenter(
          350, //TODO
          350 //TODO
        )
      );

    const linkElements = svg
      .append("g")
      .selectAll("line")
      .data(links)
      .enter()
      .append("line")
      .attr("stroke-width", 2)
      .attr("stroke", "#E5E5E5");

    const nodeElements = svg
      .append("g")
      .on("click", function (d) {
        //TODO: might be useful eventually to show some more data than url on click / or on hover potentially
        const currentlySelectedNode = graph.nodes.find(
          (node) => node.url === d.target.id
        );
      })
      .on("mouseover", function (d) {
        console.log("onhover");
        console.log(d.explicitOriginalTarget.id);

        d3.select(this)
          .attr("r", 20)
          .attr("fill", "#ffd95c")
          .append("text")
          .attr("x", function () {
            return d.x;
          })
          .attr("y", function () {
            return d.y;
          })
          .attr("font-size", 8)
          .style("fill", "black")
          .attr("class", "text-on-hover")
          .text(() => d.explicitOriginalTarget.id);
      })
      .on("mouseout", function (d) {
        console.log("onout");
        d3.selectAll(".text-on-hover").remove();
      })
      .selectAll("circle")
      .data(nodes)
      .enter()
      .append("circle")
      .attr("id", function (d) {
        return d.url;
      })
      .attr("r", 10)
      .attr("fill", "#f2c32b");

    /*     const textElements = svg
      .append("g")
      .selectAll("text")
      .data(nodes)
      .enter()
      .append("text")
      .text((node) => node.url)
      .attr("font-size", 15)
      .attr("dx", 15)
      .attr("dy", 4); */

    simulation.nodes(nodes).on("tick", () => {
      nodeElements.attr("cx", (node) => node.x).attr("cy", (node) => node.y);
      //textElements.attr("x", (node) => node.x).attr("y", (node) => node.y);

      linkElements
        .attr("x1", (link) => link.source.x)
        .attr("y1", (link) => link.source.y)
        .attr("x2", (link) => link.target.x)
        .attr("y2", (link) => link.target.y);
    });

    simulation.force(
      "link",
      d3
        .forceLink(links)
        .id((link) => link.url)
        .distance(60)
    );
  }

  componentDidMount() {
    this.getGraphAsync().then((graph) => this.renderGraph(graph));

    this.setState({ loading: false });
  }

  async getGraphAsync() {
    const graphsJson = [];

    console.log(this.props.ids);

    for (const id of this.props.ids) {
      console.log(id);
      const response = await fetch(`/Record/${id}/livegraph`);
      console.log(response);

      if (response.ok) {
        const graphJson = await response.json();
        graphsJson.push(graphJson);
        console.log(graphJson);
      }
    }

    const graph = this.convertGraphsJsonToD3Json(graphsJson);
    return graph;
  }

  convertGraphsJsonToD3Json(graphsJson) {
    console.log(graphsJson);
    const graph = {
      nodes: [],
      links: [],
    };

    let graphId = 0;
    for (const graphJson of graphsJson) {
      graphId++;
      for (const node of graphJson.Graph) {
        const alreadyPresentNode = graph.nodes.find((n) => n.url === node.Url);

        if (alreadyPresentNode) {
          alreadyPresentNode.inWhichGraphs.push(graphId);
        } else {
          graph.nodes.push({
            url: node.Url,
            title: node.Title,
            crawlTime: node.CrawlTime,
            inWhichGraphs: [graphId],
          });
        }

        for (const neighbourUrl of node.Neighbours) {
          graph.links.push({
            source: node.Url,
            target: neighbourUrl,
            forWhichGraph: graphId,
          });
        }
      }
    }

    console.log(graph);
    return graph;
  }

  render() {
    let loading = "";
    if (this.state.loading) {
      loading = (
        <p>
          <em>Loading ...</em>
        </p>
      );
    }

    return (
      <>
        {loading}
        <svg style={{ backgroundColor: "aqua" }}></svg>
      </>
    );
  }
}
